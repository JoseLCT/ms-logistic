using System.Globalization;
using MsLogistic.Infrastructure;
using MsLogistic.WebApi.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

CultureInfo culture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.Host.AddLogging();

builder.Services.AddControllers();
builder.Services.AddRoutePrefix("api/logistic");
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);

WebApplication app = builder.Build();

app.ApplyMigrations();

app.MapPrometheusScrapingEndpoint();
app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/", () => "MsLogistic API is running...");

app.Run();
