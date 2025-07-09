namespace Demographix.Api.Models;

public record Demographics
{
	public required string Name { get; init; }
	public int? Age { get; init; }
	public string? Gender { get; init; }
	public float? GenderProbability { get; init; }
	public List<Nationality> Nationalities { get; init; } = [];
}