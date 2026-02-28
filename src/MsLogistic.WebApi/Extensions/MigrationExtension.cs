using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MsLogistic.Infrastructure.Persistence;

namespace MsLogistic.WebApi.Extensions;

public static class MigrationExtension {
    public static void ApplyMigrations(this WebApplication app) {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
        db.Migrate();
    }
}
