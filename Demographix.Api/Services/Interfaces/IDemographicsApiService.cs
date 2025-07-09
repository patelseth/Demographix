using Demographix.Api.Models;

namespace Demographix.Api.Services.Interfaces;

public interface IDemographicsApiService
{
	Task<Demographics> FetchDemographicsAsync(string name, CancellationToken cancellationToken = default);
}