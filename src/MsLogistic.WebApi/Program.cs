using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MsLogistic.Infrastructure;
using MsLogistic.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.ApplyMigrations();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();

app.MapGet("/", () => "MsLogistic API is running...");

app.Run();