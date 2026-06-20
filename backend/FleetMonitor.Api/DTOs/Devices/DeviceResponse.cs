namespace FleetMonitor.Api.DTOs.Devices;
public class DeviceResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double LastLat { get; set; }
    public double LastLng { get; set; }
    public double FuelLevel { get; set; }
    public double FuelConsumptionRate { get; set; }
    public double Temperature { get; set; }
    public double Speed { get; set; }
    public DateTime UpdatedAt { get; set; }
}