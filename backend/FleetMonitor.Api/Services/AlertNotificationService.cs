using FleetMonitor.Api.Domain.Entities;
using FleetMonitor.Api.DTOs.Alerts;
using FleetMonitor.Api.DTOs.Devices;
using FleetMonitor.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FleetMonitor.Api.Services;

public class AlertNotificationService(IHubContext<AlertsHub> hubContext)
{
    public Task NotifyNewAlertAsync(Alert alert, Device device)
    {
        var payload = new AlertsResponse
        {
            Id = alert.Id,
            DeviceId = alert.DeviceId,
            Type = alert.Type,
            EstimatedMinutesRemaining = alert.EstimatedMinutesRemaining,
            CreatedAt = alert.CreatedAt,
            Acknowledged = alert.Acknowledged,
            Device = new DeviceResponse
            {
                Id = device.Id.ToString(),
                Name = device.Name,
                LastLat = device.LastLat,
                LastLng = device.LastLng,
                FuelLevel = device.FuelLevel,
                FuelConsumptionRate = device.FuelConsumptionRate,
                Temperature = device.Temperature,
                Speed = device.Speed,
                UpdatedAt = device.UpdatedAt
            }
        };

        return hubContext.Clients.Group(AlertsHub.AdminGroup).SendAsync("NewAlert", payload);
    }
}
