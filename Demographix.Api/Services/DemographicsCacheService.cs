using Demographix.Api.Models;
using Demographix.Api.Services.Interfaces;

using Microsoft.Extensions.Caching.Memory;

namespace Demographix.Api.Services;

public class DemographicsCacheService(IMemoryCache cache) : IDemographicsCacheService
{
	public bool TryGet(string key, out Demographics? demographics)
	{
		return cache.TryGetValue(key, out demographics);
	}

	public void Set(string key, Demographics demographics, TimeSpan? absoluteExpiration = null)
	{
		var options = new MemoryCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = absoluteExpiration ?? TimeSpan.FromMinutes(10)
		};

		cache.Set(key, demographics, options);
	}
}