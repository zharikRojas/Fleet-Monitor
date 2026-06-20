using System.Text;

namespace FleetMonitor.Api.Domain.Helpers;
public static class Base64Url {
    
public static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

public static byte[] Base64UrlDecode(string input)
{
    var padded = input
        .Replace('-', '+')
        .Replace('_', '/');

    switch (padded.Length % 4)
    {
        case 2: padded += "=="; break;
        case 3: padded += "="; break;
    }

    return Convert.FromBase64String(padded);
}

public static string Base64UrlEncode(string text)
    => Base64UrlEncode(Encoding.UTF8.GetBytes(text)); 

}