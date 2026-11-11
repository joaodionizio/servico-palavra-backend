using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ServicoPalavra.Api.RateLimiting;

/// <summary>
/// Stable keys for per-client rate-limit partitions. Forwarded headers must be
/// processed before these helpers run, so RemoteIpAddress is trustworthy.
/// </summary>
public static class RateLimitPartitionKey
{
    public static string ForIp(HttpContext context)
    {
        var address = context.Connection.RemoteIpAddress;
        if (address is null)
        {
            return "ip:unknown";
        }

        // Treat IPv4 and IPv4-mapped IPv6 addresses as the same client.
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        return $"ip:{address}";
    }

    public static string ForUserOrIp(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
            {
                return $"user:{userId}";
            }
        }

        return ForIp(context);
    }
}
