using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FleetMonitor.Api.Domain.Entities;
using FleetMonitor.Api.Domain.Enums;
using FleetMonitor.Api.Domain.Helpers;
using FleetMonitor.Api.Domain.Models;

namespace FleetMonitor.Api.Infrastructure.Auth;

public class JwtService(IConfiguration configuration)
{
    private readonly string _secretKey = configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey no esta configurado");
    private readonly string _issuer = configuration["Jwt:Issuer"] ?? string.Empty;
    private readonly string _audience = configuration["Jwt:Audience"] ?? string.Empty;
    private readonly int _expirationMinutes = int.Parse(configuration["Jwt:ExpirationMinutes"] ?? "60");

    public (string Token, DateTime ExpiresAt) GenerateToken(User user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes);
        var header = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" });

        var payload = JsonSerializer.Serialize(new
        {
            sub = user.Id.ToString(),
            email = user.Email,
            role = user.Role.ToString(),
            iss = _issuer,
            aud = _audience,
            exp = (long)(expiresAt - DateTime.UnixEpoch).TotalSeconds
        });

        var encodedHeader = Base64Url.Base64UrlEncode(header);
        var encodedPayload = Base64Url.Base64UrlEncode(payload);
        var data = $"{encodedHeader}.{encodedPayload}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        var signatureBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        var encodedSignature = Base64Url.Base64UrlEncode(signatureBytes);

        return ($"{data}.{encodedSignature}", expiresAt);
    }

    public JwtClaims? ValidateToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        var data = $"{parts[0]}.{parts[1]}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        var received = Base64Url.Base64UrlDecode(parts[2]);

        if (!CryptographicOperations.FixedTimeEquals(computed, received))
        {
            return null;
        }

        var payloadJson = Encoding.UTF8.GetString(Base64Url.Base64UrlDecode(parts[1]));
        using var doc = JsonDocument.Parse(payloadJson);
        var root = doc.RootElement;

        var exp = root.GetProperty("exp").GetInt64();
        if (exp < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        {
            return null;
        }

        var sub = root.GetProperty("sub").GetString();
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
        {
            return null;
        }

        var roleText = root.GetProperty("role").GetString();
        if (string.IsNullOrEmpty(roleText) || !Enum.TryParse<UserRole>(roleText, out var role))
        {
            return null;
        }

        return new JwtClaims
        {
            UserId = userId,
            Email = root.GetProperty("email").GetString() ?? string.Empty,
            Role = role
        };
    }
}
