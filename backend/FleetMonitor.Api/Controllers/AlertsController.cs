using FleetMonitor.Api.Domain.Enums;
using FleetMonitor.Api.Middleware;
using FleetMonitor.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetMonitor.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController(FuelAlertService fuelAlertService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllAlerts()
    {
        var role = HttpContext.GetUserRole();
        if(role != UserRole.Admin)
        {
            return Forbid();
        }
        var alerts = await fuelAlertService.GetAllAlertsAsync();
        return Ok(alerts);
    }

    
}
