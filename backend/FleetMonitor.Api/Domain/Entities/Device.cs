namespace FleetMonitor.Api.Domain.Entities;

public class Device
{
    public Guid Id { get; set; }
    public required string MaskedId { get; set; }
    public required string Name { get; set; }
    public double LastLat { get; set; }
    public double LastLng { get; set; }
    public double FuelLevel { get; set; }
    public double FuelConsumptionRate { get; set; }
    public double Temperature { get; set; }
    public double Speed { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<SensorReading> Readings { get; set; } = [];
    public ICollection<Alert> Alerts { get; set; } = [];
}
