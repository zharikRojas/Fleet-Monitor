using FleetMonitor.Api.Domain.Entities;
using FleetMonitor.Api.Domain.Enums;
using FleetMonitor.Api.DTOs.Devices;
using FleetMonitor.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Api.Services;

public class DeviceService(AppDbContext context)
{
    public async Task<List<DeviceResponse>> GetAllDevicesAsync(UserRole role)
    {
        var devices = await context.Devices.AsNoTracking().ToListAsync();
        return devices.Select(d => MapDevice(d, role)).ToList();
    }

    public async Task<List<SensorReadingResponse>?> GetReadingsAsync(
        string deviceId,
        UserRole role,
        DateTime? from,
        DateTime? to)
    {
        var device = await FindDeviceAsync(deviceId, role);
        if (device is null)
        {
            return null;
        }

        var query = context.SensorReadings
            .AsNoTracking()
            .Where(r => r.DeviceId == device.Id);

        if (from.HasValue)
        {
            query = query.Where(r => r.Timestamp >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(r => r.Timestamp <= to.Value);
        }

        return await query
            .OrderBy(r => r.Timestamp)
            .Select(r => new SensorReadingResponse
            {
                Timestamp = r.Timestamp,
                Lat = r.Lat,
                Lng = r.Lng,
                Fuel = r.Fuel,
                Temperature = r.Temperature,
                Speed = r.Speed
            })
            .ToListAsync();
    }

    private async Task<Device?> FindDeviceAsync(string deviceId, UserRole role)
    {
        if (role == UserRole.Admin)
        {
            if (!Guid.TryParse(deviceId, out var guid))
            {
                return null;
            }

            return await context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.Id == guid);
        }

        return await context.Devices.AsNoTracking().FirstOrDefaultAsync(d => d.MaskedId == deviceId);
    }

    private static DeviceResponse MapDevice(Device device, UserRole role)
    {
        var visibleId = role == UserRole.Admin ? device.Id.ToString() : device.MaskedId;

        return new DeviceResponse
        {
            Id = visibleId,
            Name = device.Name,
            LastLat = device.LastLat,
            LastLng = device.LastLng,
            FuelLevel = device.FuelLevel,
            FuelConsumptionRate = device.FuelConsumptionRate,
            Temperature = device.Temperature,
            Speed = device.Speed,
            UpdatedAt = device.UpdatedAt
        };
    }
}
