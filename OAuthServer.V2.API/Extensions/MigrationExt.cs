using Microsoft.EntityFrameworkCore;
using OAuthServer.V2.Data;

namespace OAuthServer.V2.API.Extensions;

public static class MigrationExt
{
    public static async Task ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }
}