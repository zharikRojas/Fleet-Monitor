using System.Security.Cryptography;
using System.Text;
using FleetMonitor.Api.Domain.Entities;
using FleetMonitor.Api.Domain.Enums;
using FleetMonitor.Api.Domain.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Api.Infrastructure.Data.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;

        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@fleetmonitor.com",
            PasswordHash = HashPassword("Admin123!"),
            Role = UserRole.Admin,
            CreatedAt = now
        };

        var viewer = new User
        {
            Id = Guid.NewGuid(),
            Email = "viewer@fleetmonitor.com",
            PasswordHash = HashPassword("Viewer123!"),
            Role = UserRole.Viewer,
            CreatedAt = now
        };

        var deviceDefinitions = new[]
        {
            new { Id = Guid.NewGuid(), Name = "Camión Norte", Lat = 4.7110, Lng = -74.0721, Fuel = 45.0, Rate = 8.5, Temp = 22.5, Speed = 55.0 },
            new { Id = Guid.NewGuid(), Name = "Camión Sur", Lat = 4.5981, Lng = -74.0758, Fuel = 6.0, Rate = 9.0, Temp = 24.0, Speed = 40.0 },
            new { Id = Guid.NewGuid(), Name = "Van Centro", Lat = 4.6533, Lng = -74.0836, Fuel = 28.0, Rate = 6.0, Temp = 21.0, Speed = 35.0 },
            new { Id = Guid.NewGuid(), Name = "Camión Occidente", Lat = 4.6900, Lng = -74.1300, Fuel = 15.0, Rate = 7.5, Temp = 23.5, Speed = 48.0 },
            new { Id = Guid.NewGuid(), Name = "Van Oriente", Lat = 4.6700, Lng = -74.0400, Fuel = 32.0, Rate = 5.5, Temp = 20.0, Speed = 30.0 }
        };

        var devices = deviceDefinitions.Select(d => new Device
        {
            Id = d.Id,
            MaskedId = DeviceIdMasker.Mask(d.Id),
            Name = d.Name,
            LastLat = d.Lat,
            LastLng = d.Lng,
            FuelLevel = d.Fuel,
            FuelConsumptionRate = d.Rate,
            Temperature = d.Temp,
            Speed = d.Speed,
            UpdatedAt = now
        }).ToList();

        var readings = new List<SensorReading>();
        var random = new Random(42);

        foreach (var device in devices)
        {
            for (var hoursAgo = 24; hoursAgo >= 0; hoursAgo--)
            {
                var timestamp = now.AddHours(-hoursAgo);
                var fuelVariation = random.NextDouble() * 2 - 1;
                var speedVariation = random.NextDouble() * 10 - 5;

                readings.Add(new SensorReading
                {
                    Id = Guid.NewGuid(),
                    DeviceId = device.Id,
                    Lat = device.LastLat + (random.NextDouble() * 0.01 - 0.005),
                    Lng = device.LastLng + (random.NextDouble() * 0.01 - 0.005),
                    Fuel = Math.Max(0, device.FuelLevel + fuelVariation - (24 - hoursAgo) * 0.3),
                    Temperature = device.Temperature + random.NextDouble() * 2 - 1,
                    Speed = Math.Max(0, device.Speed + speedVariation),
                    Timestamp = timestamp
                });
            }
        }

        context.Users.AddRange(admin, viewer);
        context.Devices.AddRange(devices);
        context.SensorReadings.AddRange(readings);

        await context.SaveChangesAsync(cancellationToken);
    }

    internal static string HashPassword(string password)
    {
        var salt = Encoding.UTF8.GetBytes("fleet-monitor-seed");
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);

        return Convert.ToBase64String(hash);
    }
}
