namespace Demographix.Api.Services.Interfaces;

public interface IRateLimiterService
{
	bool IsAllowed(string clientId);
}