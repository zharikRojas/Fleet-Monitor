using FleetMonitor.Api.Domain.Entities;
using FleetMonitor.Api.Domain.Enums;
using FleetMonitor.Api.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;

namespace FleetMonitor.Tests.Infrastructure.Auth;

public class JwtServiceTests
{
    [Fact]
    public void GenerateToken_ThenValidateToken_ReturnsSameClaims()
    {
        // Arrange
        var jwtService = CreateJwtService();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@fleetmonitor.com",
            PasswordHash = "hash",
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var (token, _) = jwtService.GenerateToken(user);
        var claims = jwtService.ValidateToken(token);

        // Assert
        Assert.NotNull(claims);
        Assert.Equal(user.Id, claims.UserId);
        Assert.Equal(user.Email, claims.Email);
        Assert.Equal(UserRole.Admin, claims.Role);
    }

    [Fact]
    public void ValidateToken_WhenTokenIsTampered_ReturnsNull()
    {
        var jwtService = CreateJwtService();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "viewer@fleetmonitor.com",
            PasswordHash = "hash",
            Role = UserRole.Viewer,
            CreatedAt = DateTime.UtcNow
        };

        var (token, _) = jwtService.GenerateToken(user);
        var tamperedToken = token[..^5] + "xxxxx";

        var claims = jwtService.ValidateToken(tamperedToken);

        Assert.Null(claims);
    }

    [Fact]
    public void ValidateToken_WhenTokenIsMalformed_ReturnsNull()
    {
        var jwtService = CreateJwtService();

        var claims = jwtService.ValidateToken("token-invalido");

        Assert.Null(claims);
    }

    private static JwtService CreateJwtService()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test-secret-key-for-unit-tests-only",
                ["Jwt:Issuer"] = "fleet-monitor-test",
                ["Jwt:Audience"] = "fleet-monitor-api-test",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();

        return new JwtService(configuration);
    }
}
