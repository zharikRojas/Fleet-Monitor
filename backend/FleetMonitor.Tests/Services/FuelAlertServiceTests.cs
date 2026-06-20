using FleetMonitor.Api.Domain.Entities;
using FleetMonitor.Api.Domain.Enums;
using FleetMonitor.Api.Infrastructure.Data;
using FleetMonitor.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Tests.Services;

public class FuelAlertServiceTests
{
    [Fact]
    public async Task CheckFuelAlertAsync_WhenRateIsZero_DoesNotCreateAlert()
    {
        // Arrange: preparar datos y dependencias
        await using var context = CreateInMemoryContext();
        var service = new FuelAlertService(context);
        var deviceId = Guid.NewGuid();

        // Act: ejecutar el metodo que quieres probar
        await service.CheckFuelAlertAsync(deviceId, fuelLevel: 10, fuelConsumptionRate: 0);

        // Assert: verificar el resultado esperado
        Assert.Empty(context.ChangeTracker.Entries<Alert>());
    }

    [Fact]
    public async Task CheckFuelAlertAsync_WhenEstimatedMinutesIs60OrMore_DoesNotCreateAlert()
    {
        await using var context = CreateInMemoryContext();
        var service = new FuelAlertService(context);

        // (30 / 6) * 60 = 300 minutos -> no debe alertar
        await service.CheckFuelAlertAsync(Guid.NewGuid(), fuelLevel: 30, fuelConsumptionRate: 6);

        Assert.Empty(context.ChangeTracker.Entries<Alert>());
    }

    [Fact]
    public async Task CheckFuelAlertAsync_WhenEstimatedMinutesIsLessThan60_CreatesAlert()
    {
        await using var context = CreateInMemoryContext();
        var service = new FuelAlertService(context);
        var deviceId = Guid.NewGuid();

        // (5 / 10) * 60 = 30 minutos -> debe alertar
        await service.CheckFuelAlertAsync(deviceId, fuelLevel: 5, fuelConsumptionRate: 10);

        var addedAlert = Assert.Single(context.ChangeTracker.Entries<Alert>());
        var alert = addedAlert.Entity;

        Assert.Equal(deviceId, alert.DeviceId);
        Assert.Equal(AlertType.LowFuel, alert.Type);
        Assert.Equal(30, alert.EstimatedMinutesRemaining);
        Assert.False(alert.Acknowledged);
    }

    [Fact]
    public async Task CheckFuelAlertAsync_WhenRecentAlertExists_DoesNotCreateDuplicate()
    {
        await using var context = CreateInMemoryContext();
        var service = new FuelAlertService(context);
        var deviceId = Guid.NewGuid();

        context.Alerts.Add(new Alert
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            Type = AlertType.LowFuel,
            EstimatedMinutesRemaining = 20,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
            Acknowledged = false
        });
        await context.SaveChangesAsync();

        await service.CheckFuelAlertAsync(deviceId, fuelLevel: 5, fuelConsumptionRate: 10);

        Assert.Single(await context.Alerts.ToListAsync());
    }

    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
