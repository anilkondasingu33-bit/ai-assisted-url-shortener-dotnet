using System.Net;

namespace UrlShortener.Api.Services;

// Separate validation keeps the main business service easy to read.
public static class UrlValidation
{
    public static string ValidatePublicHttpUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 2048 ||
            !Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            string.IsNullOrEmpty(uri.Host) || !string.IsNullOrEmpty(uri.UserInfo))
            throw new ApiException(400, "A valid absolute HTTP(S) URL is required (maximum 2048 characters).");

        var host = uri.IdnHost.TrimEnd('.');
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
            host.EndsWith(".local", StringComparison.OrdinalIgnoreCase) ||
            !host.Contains('.'))
            throw new ApiException(400, "Local network destinations are not allowed.");

        if (IPAddress.TryParse(host, out var ip) &&
            (IPAddress.IsLoopback(ip) || IsPrivateOrReservedIp(ip)))
            throw new ApiException(400, "Private or reserved IP destinations are not allowed.");
        return uri.AbsoluteUri;
    }

    private static bool IsPrivateOrReservedIp(IPAddress ip)
    {
        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            if (ip.IsIPv4MappedToIPv6) return IsPrivateOrReservedIp(ip.MapToIPv4());
            var bytes6 = ip.GetAddressBytes();
            return ip.IsIPv6LinkLocal || ip.IsIPv6Multicast ||
                (bytes6[0] & 0xfe) == 0xfc || ip.Equals(IPAddress.IPv6Any);
        }
        var b = ip.GetAddressBytes();
        return b[0] == 0 || b[0] == 10 || b[0] == 127 || b[0] >= 224 ||
            (b[0] == 169 && b[1] == 254) || (b[0] == 172 && b[1] is >= 16 and <= 31) ||
            (b[0] == 192 && b[1] == 168) || (b[0] == 100 && b[1] is >= 64 and <= 127) ||
            (b[0] == 192 && b[1] == 0) || (b[0] == 198 && b[1] is 18 or 19);
    }
}
