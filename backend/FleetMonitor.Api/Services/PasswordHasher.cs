using System.Security.Cryptography;
using System.Text;

namespace FleetMonitor.Api.Services;

public static class PasswordHasher
{
    private static readonly byte[] Salt = Encoding.UTF8.GetBytes("fleet-monitor-seed");
    public static string Hash(string password)
    {
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            Salt,
            iterations: 100_000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32);
        return Convert.ToBase64String(hash);
    }
    public static bool Verify(string password, string storedHash)
        => Hash(password) == storedHash;
}