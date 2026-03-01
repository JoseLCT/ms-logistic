using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MsLogistic.Infrastructure;
using MsLogistic.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

var culture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

builder.Host.AddLogging();

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

app.ApplyMigrations();

app.MapPrometheusScrapingEndpoint();
app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.MapGet("/", () => "MsLogistic API is running...");

app.Run();
