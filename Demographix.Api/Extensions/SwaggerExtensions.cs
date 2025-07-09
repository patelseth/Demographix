using Microsoft.OpenApi.Models;

using System.Reflection;

namespace Demographix.Api.Extensions;

public static class SwaggerExtensions
{
	public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
	{
		var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
		var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

		services.AddSwaggerGen(options =>
		{
			options.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "Demographix API",
				Version = "v1",
				Description = "An API that returns demographic information (age, gender, nationality) for a given name."
			});

			options.IncludeXmlComments(xmlPath);
		});

		services.AddEndpointsApiExplorer();

		return services;
	}
}