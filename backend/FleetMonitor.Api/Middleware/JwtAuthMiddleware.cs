using FleetMonitor.Api.Infrastructure.Auth;
using System.Text.Json;

namespace FleetMonitor.Api.Middleware;

public class JwtAuthMiddleware(RequestDelegate next, JwtService jwtService)
{
    private static readonly PathString LoginPath = new("/api/auth/login");

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPublicPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        var token = ExtractBearerToken(context.Request.Headers.Authorization)
            ?? ExtractAccessTokenFromQuery(context.Request);
        if (token is null)
        {
            await WriteUnauthorizedAsync(context, "Token invalido o expirado");
            return;
        }

        var claims = jwtService.ValidateToken(token);
        if (claims is null)
        {
            await WriteUnauthorizedAsync(context, "Token invalido o expirado");
            return;
        }

        context.Items["UserId"] = claims.UserId;
        context.Items["UserRole"] = claims.Role;

        await next(context);
    }

    private static bool IsPublicPath(PathString path)
    {
        return path.StartsWithSegments(LoginPath)
            || path.StartsWithSegments("/swagger");
    }

    private static string? ExtractBearerToken(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return authorizationHeader["Bearer ".Length..].Trim();
    }

    private static string? ExtractAccessTokenFromQuery(HttpRequest request)
    {
        if (!request.Path.StartsWithSegments("/hubs"))
        {
            return null;
        }

        var accessToken = request.Query["access_token"].FirstOrDefault();
        return string.IsNullOrWhiteSpace(accessToken) ? null : accessToken.Trim();
    }

    private static async Task WriteUnauthorizedAsync(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message }));
    }
}
