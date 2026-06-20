using FleetMonitor.Api.DTOs.Auth;
using FleetMonitor.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetMonitor.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Email y contraseña son requeridos" });
        }

        var result = await authService.LoginAsync(request);
        if (result is null)
        {
            return Unauthorized(new { message = "Credenciales invalidas" });
        }

        return Ok(result);
    }
}
