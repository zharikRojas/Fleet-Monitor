using FleetMonitor.Api.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Api.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            await context.Database.MigrateAsync();
            await DbSeeder.SeedAsync(context);
            logger.LogInformation("Migraciones y seeders aplicados correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ha ocurrido un error mientras se aplicaban migraciones o seeders a la base de datos.");
            throw;
        }
    }
}
