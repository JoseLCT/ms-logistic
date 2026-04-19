using Serilog;
using Serilog.Enrichers.Span;

namespace MsLogistic.WebApi.Extensions;

public static class LoggingExtension {
	public static IHostBuilder AddLogging(this IHostBuilder builder) {
		return builder.UseSerilog((ctx, services, config) => {
			config
				.ReadFrom.Configuration(ctx.Configuration)
				.ReadFrom.Services(services)
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithSpan()
				.Enrich.WithProperty("Application", "MsLogistic.WebApi");
		});
	}
}
