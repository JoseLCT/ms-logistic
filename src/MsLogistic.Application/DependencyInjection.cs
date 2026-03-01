using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MsLogistic.Application.Routes.Services;
using MsLogistic.Application.Shared.Behaviors;

namespace MsLogistic.Application;

public static class DependencyInjection {
    public static IServiceCollection AddApplication(this IServiceCollection services) {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
        );

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainExceptionBehavior<,>));

        services.AddScoped<RouteCompletionService>();

        return services;
    }
}
