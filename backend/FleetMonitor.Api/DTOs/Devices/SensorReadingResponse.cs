namespace FleetMonitor.Api.DTOs.Devices;
public class SensorReadingResponse
{
    public DateTime Timestamp { get; set; }
    public double Lat { get; set; }
    public double Lng { get; set; }
    public double Fuel { get; set; }
    public double Temperature { get; set; }
    public double Speed { get; set; }
}