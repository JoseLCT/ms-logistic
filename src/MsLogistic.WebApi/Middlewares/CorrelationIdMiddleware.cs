using Serilog.Context;

namespace MsLogistic.WebApi.Middlewares;

public class CorrelationIdMiddleware(RequestDelegate next) {
	private const string HeaderKey = "X-Correlation-Id";

	public async Task InvokeAsync(HttpContext context) {
		string correlationId = context.Request.Headers[HeaderKey].FirstOrDefault() ?? Guid.NewGuid().ToString("N")[..8];

		context.Response.Headers[HeaderKey] = correlationId;

		using (LogContext.PushProperty("CorrelationId", correlationId)) {
			await next(context);
		}
	}
}
