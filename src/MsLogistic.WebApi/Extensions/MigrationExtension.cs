using MsLogistic.Infrastructure.Persistence;

namespace MsLogistic.WebApi.Extensions;

public static class MigrationExtension {
	public static void ApplyMigrations(this WebApplication app) {
		using IServiceScope scope = app.Services.CreateScope();
		IDatabase db = scope.ServiceProvider.GetRequiredService<IDatabase>();
		db.Migrate();
	}
}
