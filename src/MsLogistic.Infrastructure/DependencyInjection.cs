using System.Reflection;
using CloudinaryDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MsLogistic.Application;
using MsLogistic.Application.Abstractions.Services;
using MsLogistic.Core.Interfaces;
using MsLogistic.Domain.Batches.Repositories;
using MsLogistic.Domain.Customers.Repositories;
using MsLogistic.Domain.DeliveryZones.Repositories;
using MsLogistic.Domain.Drivers.Repositories;
using MsLogistic.Domain.Orders.Repositories;
using MsLogistic.Domain.Products.Repositories;
using MsLogistic.Domain.Routes.Repositories;
using MsLogistic.Infrastructure.External.Cloudinary;
using MsLogistic.Infrastructure.External.GoogleMaps;
using MsLogistic.Infrastructure.Persistence;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using MsLogistic.Infrastructure.Persistence.Repositories;

namespace MsLogistic.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
        services.AddApplication().AddPersistence(configuration);

        services.AddCloudinary(configuration);
        services.AddGoogleMaps(configuration);

        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly())
        );

        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration) {
        var dbConnectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<PersistenceDbContext>(context =>
            context.UseNpgsql(dbConnectionString, npgsqlOptions => { npgsqlOptions.UseNetTopologySuite(); }));

        services.AddDbContext<DomainDbContext>(context =>
            context.UseNpgsql(dbConnectionString, npgsqlOptions => { npgsqlOptions.UseNetTopologySuite(); }));

        services.AddScoped<IDatabase, DomainDbContext>();

        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IDeliveryZoneRepository, DeliveryZoneRepository>();
        services.AddScoped<IDriverRepository, DriverRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }

    private static IServiceCollection AddCloudinary(
        this IServiceCollection services,
        IConfiguration configuration
    ) {
        services.Configure<CloudinaryOptions>(
            configuration.GetSection(CloudinaryOptions.SectionName)
        );

        services.AddSingleton(sp => {
            var options = sp.GetRequiredService<IOptions<CloudinaryOptions>>().Value;
            var account = new Account(options.CloudName, options.ApiKey, options.ApiSecret);
            return new Cloudinary(account);
        });

        services.AddScoped<IImageStorageService, CloudinaryImageStorageService>();

        return services;
    }

    private static IServiceCollection AddGoogleMaps(
        this IServiceCollection services,
        IConfiguration configuration
    ) {
        services.Configure<GoogleMapsOptions>(
            configuration.GetSection(GoogleMapsOptions.SectionName)
        );

        services.AddHttpClient<IRouteCalculator, GoogleMapsRouteCalculator>(client => {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }
}
