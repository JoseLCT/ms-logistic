using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MsLogistic.Application;
using MsLogistic.Core.Abstractions;
using MsLogistic.Domain.Customer.Repositories;
using MsLogistic.Domain.DeliveryPerson.Repositories;
using MsLogistic.Domain.DeliveryZone.Repositories;
using MsLogistic.Domain.Order.Repositories;
using MsLogistic.Domain.Product.Repositories;
using MsLogistic.Domain.Route.Repositories;
using MsLogistic.Infrastructure.Persistence;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.Repositories;

namespace MsLogistic.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication().AddPersistence(configuration);
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
        );

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<PersistenceDbContext>(context =>
            context.UseNpgsql(dbConnectionString, npgsqlOptions => { npgsqlOptions.UseNetTopologySuite(); }));

        services.AddDbContext<DomainDbContext>(context =>
            context.UseNpgsql(dbConnectionString, npgsqlOptions => { npgsqlOptions.UseNetTopologySuite(); }));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IDeliveryPersonRepository, DeliveryPersonRepository>();
        services.AddScoped<IDeliveryZoneRepository, DeliveryZoneRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}