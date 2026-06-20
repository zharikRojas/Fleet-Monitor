using FleetMonitor.Api.Domain.Enums;

namespace FleetMonitor.Api.Domain.Models;
public class JwtClaims
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}