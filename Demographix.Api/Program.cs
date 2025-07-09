using Demographix.Api.Extensions;
using Demographix.Api.Services;
using Demographix.Api.Services.Interfaces;

using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();

builder.Services.AddScoped<IDemographicsCacheService, DemographicsCacheService>();
builder.Services.AddScoped<IDemographicsApiService, DemographicsApiService>();

builder.Services.AddSingleton<IRateLimiterService>(sp =>
	new RateLimiterService(sp.GetRequiredService<IMemoryCache>(), maxRequests: 20, window: TimeSpan.FromMinutes(1)));

builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();