using FleetMonitor.Api.Domain.Entities;
using FleetMonitor.Api.DTOs.Sensors;
using FleetMonitor.Api.Infrastructure.Data;

namespace FleetMonitor.Api.Services;

public class SensorIngestService(AppDbContext context, FuelAlertService fuelAlertService)
{

    public async Task<bool> IngestSensorReadingsAsync(IngestSensorRequest request)
    {
        var device = await context.Devices.FindAsync(request.DeviceId);
        if (device is null)
        {
            return false;
        }
        device.LastLat = request.Lat;
        device.LastLng = request.Lng;
        device.FuelLevel = request.Fuel;
        device.Temperature = request.Temperature;
        device.Speed = request.Speed;
        device.UpdatedAt = DateTime.UtcNow;

        context.SensorReadings.Add(new SensorReading
        {
            Id = Guid.NewGuid(),
            DeviceId = request.DeviceId,
            Lat = request.Lat,
            Lng = request.Lng,
            Fuel = request.Fuel,
            Temperature = request.Temperature,
            Speed = request.Speed,
            Timestamp = DateTime.UtcNow
        });
        await fuelAlertService.CheckFuelAlertAsync(request.DeviceId, request.Fuel, device.FuelConsumptionRate);

        await context.SaveChangesAsync();
        return true;
   
    }
}
