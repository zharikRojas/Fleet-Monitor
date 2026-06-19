namespace FleetMonitor.Api.Domain.Entities;

public class SensorReading
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double Fuel { get; set; }
    public double Temperature { get; set; }
    public double Speed { get; set; }
    public DateTime Timestamp { get; set; }

    public Device Device { get; set; } = null!;
}
