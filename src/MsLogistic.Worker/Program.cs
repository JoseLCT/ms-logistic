using Joselct.Outbox.MediatR.Extensions;
using Microsoft.Extensions.Hosting;
using MsLogistic.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOutboxWorker();

var host = builder.Build();

host.Run();
