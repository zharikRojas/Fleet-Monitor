using FleetMonitor.Api.Domain.Enums;
using FleetMonitor.Api.DTOs.Devices;

namespace FleetMonitor.Api.DTOs.Alerts;
public class AlertsResponse
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public AlertType Type { get; set; }
    public double EstimatedMinutesRemaining { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Acknowledged { get; set; }
    public DeviceResponse Device { get; set; } = null!;
}