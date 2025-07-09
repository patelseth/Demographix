using System.Text.Json.Serialization;

namespace Demographix.Api.Models;

public record NationalizeResponse(
	string Name,
	[property: JsonPropertyName("country")]
	List<Nationality> Nationality
);