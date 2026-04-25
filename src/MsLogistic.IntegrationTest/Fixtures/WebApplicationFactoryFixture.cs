using System.Security.Claims;
using System.Text.Encodings.Web;
using Consul;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MsLogistic.Infrastructure.Persistence.DomainModel;
using MsLogistic.Infrastructure.Persistence.PersistenceModel;
using Testcontainers.PostgreSql;
using Xunit;

namespace MsLogistic.IntegrationTest.Fixtures;

public class WebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime {
	private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
		.WithImage("postgis/postgis:16-3.4")
		.WithDatabase("logistic_test_db")
		.WithUsername("test_user")
		.WithPassword("test_password")
		.WithCleanUp(true)
		.Build();

	public async Task InitializeAsync() {
		await _postgresContainer.StartAsync();
	}

	async Task IAsyncLifetime.DisposeAsync() {
		await _postgresContainer.DisposeAsync();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder) {
		builder.ConfigureServices(services => {
			// Remove existing DbContext registrations
			services.RemoveAll<DbContextOptions<PersistenceDbContext>>();
			services.RemoveAll<DbContextOptions<DomainDbContext>>();
			services.RemoveAll<PersistenceDbContext>();
			services.RemoveAll<DomainDbContext>();

			string connectionString = _postgresContainer.GetConnectionString();

			services.AddDbContext<PersistenceDbContext>(options =>
				options.UseNpgsql(connectionString, npgsqlOptions => {
					npgsqlOptions.UseNetTopologySuite();
					npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "persistence");
				})
			);

			services.AddDbContext<DomainDbContext>(options =>
				options.UseNpgsql(connectionString, npgsqlOptions => {
					npgsqlOptions.UseNetTopologySuite();
					npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "domain");
				})
			);

			services.RemoveAll<IHostedService>();

			services.RemoveAll<IConsulClient>();
			services.AddSingleton<IConsulClient>(new ConsulClient());

			services.AddAuthentication("Test")
				.AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>("Test", _ => { });

			ServiceProvider serviceProvider = services.BuildServiceProvider();
			using IServiceScope scope = serviceProvider.CreateScope();
			IServiceProvider scopedServices = scope.ServiceProvider;

			PersistenceDbContext persistenceDb = scopedServices.GetRequiredService<PersistenceDbContext>();
			DomainDbContext domainDb = scopedServices.GetRequiredService<DomainDbContext>();

			persistenceDb.Database.Migrate();
			domainDb.Database.Migrate();
		});

		builder.UseEnvironment("Test");
	}
}

internal class FakeAuthHandler(
	IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder) {
	protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
		Claim[] claims = [
			new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
			new Claim(ClaimTypes.Name, "Test User"),
		];
		var identity = new ClaimsIdentity(claims, "Test");
		var principal = new ClaimsPrincipal(identity);
		var ticket = new AuthenticationTicket(principal, "Test");

		return Task.FromResult(AuthenticateResult.Success(ticket));
	}
}
