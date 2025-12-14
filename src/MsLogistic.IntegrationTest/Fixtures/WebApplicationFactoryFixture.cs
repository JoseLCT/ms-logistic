using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using Testcontainers.PostgreSql;

namespace MsLogistic.IntegrationTest.Fixtures;

public class WebApplicationFactoryFixture : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _postgresContainer;

    public WebApplicationFactoryFixture()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:16-3.4")
            .WithDatabase("logistic_test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true)
            .WithReuse(true)
            .Build();

        _postgresContainer.StartAsync().GetAwaiter().GetResult();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing DbContext registrations
            services.RemoveAll<DbContextOptions<PersistenceDbContext>>();
            services.RemoveAll<DbContextOptions<DomainDbContext>>();
            services.RemoveAll<PersistenceDbContext>();
            services.RemoveAll<DomainDbContext>();

            var connectionString = _postgresContainer.GetConnectionString();

            services.AddDbContext<PersistenceDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.UseNetTopologySuite();
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "persistence");
                    }
                )
            );

            services.AddDbContext<DomainDbContext>(options =>
                options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.UseNetTopologySuite();
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "domain");
                    }
                )
            );

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var persistenceDb = scopedServices.GetRequiredService<PersistenceDbContext>();
            var domainDb = scopedServices.GetRequiredService<DomainDbContext>();

            persistenceDb.Database.Migrate();
            domainDb.Database.Migrate();
        });

        builder.UseEnvironment("Test");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _postgresContainer.DisposeAsync().GetAwaiter().GetResult();
    }
}