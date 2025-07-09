using Demographix.Api.Models;

namespace Demographix.Api.Services.Interfaces;

public interface IDemographicsCacheService
{
	bool TryGet(string key, out Demographics? demographics);
	void Set(string key, Demographics demographics, TimeSpan? absoluteExpiration = null);
}