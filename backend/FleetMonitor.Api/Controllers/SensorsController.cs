using FleetMonitor.Api.Domain.Enums;
using FleetMonitor.Api.DTOs.Sensors;
using FleetMonitor.Api.Middleware;
using FleetMonitor.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetMonitor.Api.Controllers;

[ApiController]
[Route("api/sensors")]
public class SensorsController(SensorIngestService sensorIngestService) : ControllerBase
{
    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest([FromBody] IngestSensorRequest request)
    {
        if(HttpContext.GetUserRole() != UserRole.Admin)
        {
            return Forbid();
        }
        var success = await sensorIngestService.IngestSensorReadingsAsync(request);
        if (!success)
        {
            return NotFound(new { message = "Dispositivo no encontrado" });
        }
        return Ok(new { message = "Ingesta exitosa" });
    }
}
