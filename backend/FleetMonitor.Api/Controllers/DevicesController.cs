using FleetMonitor.Api.Middleware;
using FleetMonitor.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetMonitor.Api.Controllers;

[ApiController]
[Route("api/devices")]
public class DevicesController(DeviceService deviceService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var role = HttpContext.GetUserRole();
        var devices = await deviceService.GetAllDevicesAsync(role);
        return Ok(devices);
    }

    [HttpGet("{id}/readings")]
    public async Task<IActionResult> GetReadings(
        string id,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var role = HttpContext.GetUserRole();
        var readings = await deviceService.GetReadingsAsync(id, role, from, to);

        if (readings is null)
        {
            return NotFound(new { message = "Dispositivo no encontrado" });
        }

        return Ok(readings);
    }
}
