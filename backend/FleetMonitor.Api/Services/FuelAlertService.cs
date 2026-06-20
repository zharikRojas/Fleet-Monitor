using FleetMonitor.Api.Domain.Entities;
using FleetMonitor.Api.Domain.Enums;
using FleetMonitor.Api.DTOs.Alerts;
using FleetMonitor.Api.DTOs.Devices;
using FleetMonitor.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Api.Services;

public class FuelAlertService(AppDbContext context)
{
    public async Task<Alert?> CheckFuelAlertAsync(Guid deviceId, double fuelLevel, double fuelConsumptionRate)
    {
        if (fuelConsumptionRate <= 0)
        {
            return null;
        }

        var estimatedMinutesRemaining = (fuelLevel / fuelConsumptionRate) * 60;

        if (estimatedMinutesRemaining >= 60)
        {
            return null;
        }

        var recentAlert = await context.Alerts.AnyAsync(a =>
            a.DeviceId == deviceId &&
            a.Type == AlertType.LowFuel &&
            a.CreatedAt > DateTime.UtcNow.AddMinutes(-15));

        if (recentAlert)
        {
            return null;
        }

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

        return alert;
    }

    public async Task<List<AlertsResponse>> GetAllAlertsAsync()
    {
        var alerts = await context.Alerts.AsNoTracking().Include(a => a.Device).OrderBy(a => a.CreatedAt).ToListAsync();
        return new List<AlertsResponse>(alerts.Select(a => new AlertsResponse
        {
            Id = a.Id,
            DeviceId = a.DeviceId,
            Type = a.Type,
            EstimatedMinutesRemaining = a.EstimatedMinutesRemaining,
            CreatedAt = a.CreatedAt,
            Acknowledged = a.Acknowledged,
            Device = new DeviceResponse
            {
                Id = a.Device.Id.ToString(),
                Name = a.Device.Name,
                LastLat = a.Device.LastLat,
                LastLng = a.Device.LastLng,
                FuelLevel = a.Device.FuelLevel,
                FuelConsumptionRate = a.Device.FuelConsumptionRate,
            }

        }).ToList());
    }

}
