namespace Demographix.Api.Models;

public record GenderizeResponse(string Name, string Gender, float? Probability, int Count);