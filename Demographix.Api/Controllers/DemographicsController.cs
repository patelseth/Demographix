using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Demographix.Api.Models;

namespace Demographix.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class DemographicsController(IHttpClientFactory httpClientFactory) : ControllerBase
	{
		private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

		/// <summary>
		/// Returns demographic data (age, gender, nationality) for a given name.
		/// </summary>
		/// <param name="name">The name to analyze</param>
		/// <returns>Combined demographic data</returns>
		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return BadRequest("Query parameter 'name' is required.");

			var client = _httpClientFactory.CreateClient();

			var agifyTask = client.GetFromJsonAsync<AgifyResponse>($"https://api.agify.io?name={name}");
			var genderizeTask = client.GetFromJsonAsync<GenderizeResponse>($"https://api.genderize.io?name={name}");
			var nationalizeTask = client.GetFromJsonAsync<NationalizeResponse>($"https://api.nationalize.io?name={name}");

			await Task.WhenAll(agifyTask, genderizeTask, nationalizeTask);

			var agify = agifyTask.Result;
			var genderize = genderizeTask.Result;
			var nationalize = nationalizeTask.Result;

			var nationalities = nationalize?.Nationality?
				.OrderByDescending(c => c.Probability)
				.Take(3)
				.Select(c => new Nationality
				{
					CountryCode = c.CountryCode,
					CountryName = GetCountryName(c.CountryCode),
					Probability = c.Probability
				})
				.ToList() ?? [];

			var result = new Demographics
			{
				Name = name,
				Age = agify?.Age,
				Gender = genderize?.Gender,
				GenderProbability = genderize?.Probability,
				Nationalities = nationalities
			};

			return Ok(result);
		}

		private static string GetCountryName(string countryCode)
		{
			try
			{
				var region = new RegionInfo(countryCode);
				return region.EnglishName;
			}
			catch
			{
				return countryCode;
			}
		}
	}
}
