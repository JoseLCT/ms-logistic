using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace MsLogistic.WebApi.Extensions;

public static class RouteConventionExtensions {
    public static IServiceCollection AddRoutePrefix(
        this IServiceCollection services,
        string prefix) {
        services.AddControllers(options => {
            options.Conventions.Add(new RoutePrefixConvention(prefix));
        });

        return services;
    }
}

internal class RoutePrefixConvention : IApplicationModelConvention {
    private readonly AttributeRouteModel _routePrefix;

    public RoutePrefixConvention(string prefix) {
        _routePrefix = new AttributeRouteModel(new RouteAttribute(prefix));
    }

    public void Apply(ApplicationModel application) {
        foreach (var controller in application.Controllers) {
            foreach (var selector in controller.Selectors) {
                selector.AttributeRouteModel = selector.AttributeRouteModel != null
                    ? AttributeRouteModel.CombineAttributeRouteModel(_routePrefix, selector.AttributeRouteModel)
                    : _routePrefix;
            }
        }
    }
}
