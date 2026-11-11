using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ServicoPalavra.Api.RateLimiting;

namespace ServicoPalavra.IntegrationTests;

public sealed class RateLimitPartitionKeyTests
{
    [Fact]
    public void Anonymous_clients_with_different_addresses_have_independent_keys()
    {
        var first = ContextWithAddress("192.0.2.10");
        var second = ContextWithAddress("192.0.2.11");

        Assert.NotEqual(RateLimitPartitionKey.ForIp(first), RateLimitPartitionKey.ForIp(second));
        Assert.NotEqual(RateLimitPartitionKey.ForUserOrIp(first), RateLimitPartitionKey.ForUserOrIp(second));
    }

    [Fact]
    public void Authenticated_users_behind_same_ip_have_independent_general_keys()
    {
        var first = ContextWithAddress("198.51.100.42", "user-one");
        var second = ContextWithAddress("198.51.100.42", "user-two");

        Assert.Equal("user:user-one", RateLimitPartitionKey.ForUserOrIp(first));
        Assert.Equal("user:user-two", RateLimitPartitionKey.ForUserOrIp(second));
        Assert.NotEqual(RateLimitPartitionKey.ForUserOrIp(first), RateLimitPartitionKey.ForUserOrIp(second));
    }

    [Fact]
    public void Authentication_budget_is_ip_based_and_not_account_based()
    {
        var first = ContextWithAddress("198.51.100.42", "user-one");
        var second = ContextWithAddress("198.51.100.42", "user-two");

        Assert.Equal(RateLimitPartitionKey.ForIp(first), RateLimitPartitionKey.ForIp(second));
    }

    [Fact]
    public void IPv4_mapped_to_IPv6_has_same_key_as_IPv4()
    {
        var ipv4 = ContextWithAddress("203.0.113.10");
        var ipv6Mapped = ContextWithAddress("::ffff:203.0.113.10");

        Assert.Equal(RateLimitPartitionKey.ForIp(ipv4), RateLimitPartitionKey.ForIp(ipv6Mapped));
    }

    [Fact]
    public void Unauthenticated_principal_does_not_use_claimed_user_id()
    {
        var context = ContextWithAddress("192.0.2.15");
        context.User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, "spoofed-id")]));

        Assert.Equal("ip:192.0.2.15", RateLimitPartitionKey.ForUserOrIp(context));
    }

    private static DefaultHttpContext ContextWithAddress(string ip, string? authenticatedId = null)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        if (authenticatedId is not null)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, authenticatedId)],
                authenticationType: "Test"));
        }

        return context;
    }
}
