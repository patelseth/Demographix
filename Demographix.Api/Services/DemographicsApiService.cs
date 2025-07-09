using Demographix.Api.Models;
using Demographix.Api.Services.Interfaces;

using System.Globalization;

namespace Demographix.Api.Services;

public class DemographicsApiService(IHttpClientFactory httpClientFactory, ILogger<DemographicsApiService> logger) : IDemographicsApiService
{
	private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
	private readonly ILogger<DemographicsApiService> _logger = logger;

	public async Task<Demographics> FetchDemographicsAsync(string name, CancellationToken cancellationToken = default)
	{
		var agifyTask = FetchFromApiAsync<AgifyResponse>($"https://api.agify.io?name={name}", cancellationToken);
		var genderizeTask = FetchFromApiAsync<GenderizeResponse>($"https://api.genderize.io?name={name}", cancellationToken);
		var nationalizeTask = FetchFromApiAsync<NationalizeResponse>($"https://api.nationalize.io?name={name}", cancellationToken);

		await Task.WhenAll(agifyTask, genderizeTask, nationalizeTask);

		var agify = await agifyTask;
		var genderize = await genderizeTask;
		var nationalize = await nationalizeTask;

		if (agify == null) _logger.LogWarning("Agify returned null for {Name}", name);
		if (genderize == null) _logger.LogWarning("Genderize returned null for {Name}", name);
		if (nationalize == null) _logger.LogWarning("Nationalize returned null for {Name}", name);

		return new Demographics
		{
			Name = name,
			Age = agify?.Age,
			Gender = genderize?.Gender,
			GenderProbability = genderize?.Probability,
			Nationalities = MapNationalities(nationalize)
		};
	}

	private async Task<T?> FetchFromApiAsync<T>(string url, CancellationToken cancellationToken) where T : class
	{
		var client = _httpClientFactory.CreateClient();

		try
		{
			return await client.GetFromJsonAsync<T>(url, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "API call failed for URL: {Url}", url);
			return null;
		}
	}

	private static List<Nationality> MapNationalities(NationalizeResponse? response)
	{
		if (response?.Nationality == null)
			return [];

		return [.. response.Nationality
			.OrderByDescending(c => c.Probability)
			.Take(3)
			.Select(c => new Nationality
			{
				CountryCode = c.CountryCode,
				CountryName = GetCountryName(c.CountryCode),
				Probability = c.Probability
			})];
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
