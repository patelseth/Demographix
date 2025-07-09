using Demographix.Api.Models;
using Demographix.Api.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

namespace Demographix.Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class DemographicsController(
		IRateLimiterService rateLimiter,
		IDemographicsCacheService cacheService,
		IDemographicsApiService demographicsApiService,
		ILogger<DemographicsController> logger) : ControllerBase
	{
		private readonly IRateLimiterService _rateLimiter = rateLimiter;
		private readonly IDemographicsCacheService _cacheService = cacheService;
		private readonly IDemographicsApiService _demographicsApiService = demographicsApiService;
		private readonly ILogger<DemographicsController> _logger = logger;

		/// <summary>
		/// Returns demographic data (age, gender, nationality) for a given name.
		/// </summary>
		/// <param name="name">The name to analyze</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Combined demographic data</returns>
		[Produces("application/json")]
		[ProducesResponseType(typeof(Demographics), 200)]
		[ProducesResponseType(400)]
		[ProducesResponseType(429)]
		[ProducesResponseType(500)]
		[HttpGet]
		public async Task<IActionResult> Get([FromQuery] string name, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(name))
				return BadRequest("Query parameter 'name' is required.");

			var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

			if (!IsRequestAllowed(clientIp))
				return StatusCode(429, "Rate limit exceeded. Try again later.");

			if (TryGetFromCache(name, out var cached))
				return Ok(cached);

			var demographics = await _demographicsApiService.FetchDemographicsAsync(name, cancellationToken);

			SetCache(name, demographics);

			return Ok(demographics);
		}

		private bool IsRequestAllowed(string clientId)
		{
			var allowed = _rateLimiter.IsAllowed(clientId);

			if (!allowed)
			{
				_logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
				Response.Headers.RetryAfter = "60";
			}

			return allowed;
		}

		private bool TryGetFromCache(string key, out Demographics? demographics)
		{
			var found = _cacheService.TryGet(key, out demographics);
			if (found)
				_logger.LogInformation("Cache hit for key: {Key}", key);
			return found;
		}

		private void SetCache(string key, Demographics demographics)
		{
			_cacheService.Set(key, demographics);
			_logger.LogInformation("Cache set for key: {Key}", key);
		}
	}
}
