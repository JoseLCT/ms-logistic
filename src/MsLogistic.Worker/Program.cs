using Joselct.Outbox.MediatR.Extensions;
using Microsoft.Extensions.Hosting;
using MsLogistic.Infrastructure;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddWorkerInfrastructure(builder.Configuration);
builder.Services.AddOutboxWorker();

IHost host = builder.Build();

host.Run();
