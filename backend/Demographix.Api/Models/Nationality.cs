using System.Text.Json.Serialization;

namespace Demographix.Api.Models;

public record Nationality
{
	[JsonPropertyName("country_id")]
	public required string CountryCode { get; init; }

	public string? CountryName { get; init; }

	[JsonPropertyName("probability")]
	public required float Probability { get; init; }
}