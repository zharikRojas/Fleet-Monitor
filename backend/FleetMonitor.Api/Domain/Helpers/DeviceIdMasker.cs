namespace FleetMonitor.Api.Domain.Helpers;

public static class DeviceIdMasker
{
    public static string Mask(Guid deviceId)
    {
        var suffix = deviceId.ToString("N")[^4..].ToUpperInvariant();
        return $"DEV-****-{suffix}";
    }
}
