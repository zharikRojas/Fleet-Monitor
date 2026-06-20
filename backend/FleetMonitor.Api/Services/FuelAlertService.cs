using FleetMonitor.Api.Domain.Entities;
using FleetMonitor.Api.Domain.Enums;
using FleetMonitor.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Api.Services;

public class FuelAlertService(AppDbContext context)
{
    public async Task CheckFuelAlertAsync(Guid deviceId, double fuelLevel, double fuelConsumptionRate)
    {
        if (fuelConsumptionRate <= 0)
        {
            return;
        }

        var estimatedMinutesRemaining = (fuelLevel / fuelConsumptionRate) * 60;

        if (estimatedMinutesRemaining >= 60)
        {
            return;
        }

        
        var recentAlert = await context.Alerts.AnyAsync(a =>
            a.DeviceId == deviceId &&
            a.Type == AlertType.LowFuel &&
            a.CreatedAt > DateTime.UtcNow.AddMinutes(-15));

        if (recentAlert) return;

        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            DeviceId = deviceId,
            Type = AlertType.LowFuel,
            EstimatedMinutesRemaining = estimatedMinutesRemaining,
            CreatedAt = DateTime.UtcNow,
            Acknowledged = false
        };
        context.Alerts.Add(alert);

    }


    
}
