using Demographix.Api.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Demographix.Api.Services;

public class RateLimiterService(IMemoryCache cache, int maxRequests = 20, TimeSpan? window = null) : IRateLimiterService
{
	private readonly TimeSpan _window = window ?? TimeSpan.FromMinutes(1);

	public bool IsAllowed(string clientId)
	{
		if (!cache.TryGetValue(clientId, out int count))
		{
			cache.Set(clientId, 1, _window);
			return true;
		}

		if (count >= maxRequests)
		{
			return false;
		}

		cache.Set(clientId, count + 1, _window);
		return true;
	}
}