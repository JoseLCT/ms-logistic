using Serilog;
using Serilog.Events;

namespace MsLogistic.WebApi.Extensions;

public static class RequestLoggingExtension {
	public static WebApplication UseRequestLogging(this WebApplication app) {
		app.UseSerilogRequestLogging(options => {
			options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} -> {StatusCode} ({Elapsed:0.0} ms)";
			options.GetLevel = (httpContext, elapsed, ex) => {
				if (ex != null || httpContext.Response.StatusCode >= 500) {
					return LogEventLevel.Error;
				}

				string path = httpContext.Request.Path.Value ?? string.Empty;
				if (path.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase) || path.StartsWith("/health", StringComparison.OrdinalIgnoreCase)) {
					return LogEventLevel.Verbose;
				}

				return LogEventLevel.Information;
			};
		});

		return app;
	}
}
