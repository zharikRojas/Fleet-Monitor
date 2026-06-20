namespace FleetMonitor.Api.DTOs.Sensors;
public class IngestSensorRequest
{
    public Guid DeviceId { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double Fuel { get; set; }
    public double Temperature { get; set; }
    public double Speed { get; set; }
}