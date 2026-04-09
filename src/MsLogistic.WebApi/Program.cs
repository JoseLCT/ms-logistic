using System.Globalization;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MsLogistic.Infrastructure;
using MsLogistic.WebApi.Extensions;
using MsLogistic.WebApi.Middlewares;
using Prometheus;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

CultureInfo culture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.Host.AddLogging();

builder.Services.AddHealthChecks()
	.AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
	.AddNpgSql(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		name: "PostgreSQL",
		tags: ["ready"]
	);

builder.Services.AddControllers();
builder.Services.AddRoutePrefix("api/logistic");
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);

WebApplication app = builder.Build();

app.ApplyMigrations();

app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseSerilogRequestLogging(options => {
	options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} -> {StatusCode} ({Elapsed:0.0} ms)";
});
app.UseHttpMetrics();
app.MapControllers();
app.MapMetrics();

app.MapHealthChecks("/health/live", new HealthCheckOptions {
	Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new HealthCheckOptions {
	Predicate = check => check.Tags.Contains("ready"),
	ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapGet("/", () => "MsLogistic API is running...");

app.Run();
