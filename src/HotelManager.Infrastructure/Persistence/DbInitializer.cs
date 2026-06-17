using HotelManager.Domain.Entities;
using HotelManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HotelManager.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying pending migrations...");
            await context.Database.MigrateAsync();
        }

        if (!context.Users.Any())
        {
            var defaultPassword = Environment.GetEnvironmentVariable("DefaultAdminPassword") ?? "Admin123!";

            logger.LogInformation("No users found. Creating default owner account...");
            logger.LogInformation("Username: admin");
            logger.LogWarning("IMPORTANT: Change the default admin password immediately after first login.");

            var owner = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword),
                Role = UserRole.Owner,
                CreatedAt = DateTime.UtcNow
            };

            context.Users.Add(owner);
            await context.SaveChangesAsync();

            logger.LogInformation("Default owner account created successfully.");
        }
    }
}
