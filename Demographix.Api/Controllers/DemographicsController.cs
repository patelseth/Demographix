using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using Demographix.Api.Models;

namespace Demographix.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class DemographicsController(IHttpClientFactory httpClientFactory, ILogger<DemographicsController> logger) : ControllerBase
	{
		private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
		private readonly ILogger<DemographicsController> logger = logger;

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

			try
			{
				await Task.WhenAll(agifyTask, genderizeTask, nationalizeTask);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error calling external APIs for name '{Name}'", name);
				return StatusCode(503, "Failed to retrieve data from external services.");
			}

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
