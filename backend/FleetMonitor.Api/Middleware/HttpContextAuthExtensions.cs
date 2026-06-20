using FleetMonitor.Api.Domain.Enums;

namespace FleetMonitor.Api.Middleware;

public static class HttpContextAuthExtensions
{
    public static UserRole GetUserRole(this HttpContext context)
        => (UserRole)context.Items["UserRole"]!;

    public static Guid GetUserId(this HttpContext context)
        => (Guid)context.Items["UserId"]!;
}
