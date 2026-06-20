
namespace FleetMonitor.Api.DTOs.Auth;
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public required DateTime ExpiresAt { get; set; }
}