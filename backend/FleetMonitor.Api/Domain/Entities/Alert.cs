using FleetMonitor.Api.Domain.Enums;

namespace FleetMonitor.Api.Domain.Entities;

public class Alert
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public AlertType Type { get; set; }
    public double EstimatedMinutesRemaining { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Acknowledged { get; set; }

    public Device Device { get; set; } = null!;
}
