using Asp.Versioning.ApiExplorer;
using Microsoft.OpenApi;
using MsLogistic.WebApi.Config;

namespace MsLogistic.WebApi.Extensions;

public static class SwaggerExtension {
	public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services) {
		services.AddSwaggerGen(options => {
			options.AddSecurityDefinition("Bearer",
				new OpenApiSecurityScheme {
					Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Type = SecuritySchemeType.Http,
					Scheme = "Bearer"
				});

			options.AddSecurityRequirement(doc =>
				new OpenApiSecurityRequirement {
					{ new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
				});
		});

		services.ConfigureOptions<ConfigureSwaggerOptions>();
		return services;
	}

	public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app) {
		app.UseSwagger();
		app.UseSwaggerUI(options => {
			IApiVersionDescriptionProvider provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

			foreach (ApiVersionDescription? description in provider.ApiVersionDescriptions.Reverse()) {
				options.SwaggerEndpoint(
					$"/swagger/{description.GroupName}/swagger.json",
					description.GroupName.ToUpperInvariant()
				);
			}
		});

		return app;
	}
}
