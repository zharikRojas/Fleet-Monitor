using FleetMonitor.Api.DTOs.Auth;
using FleetMonitor.Api.Infrastructure.Auth;
using FleetMonitor.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FleetMonitor.Api.Services;

public class AuthService(AppDbContext context, JwtService jwtService)
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var (token, expiresAt) = jwtService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            Email = user.Email,
            Role = user.Role.ToString(),
            ExpiresAt = expiresAt
        };
    }
}
