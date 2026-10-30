using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServicoPalavra.Infrastructure.Persistence;

namespace ServicoPalavra.IntegrationTests;

public sealed class ObservabilityTests : IClassFixture<SecurityWebApplicationFactory>
{
    private readonly SecurityWebApplicationFactory _factory;

    public ObservabilityTests(SecurityWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Health_response_exposes_server_timing_without_external_service()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
        Assert.True(response.Headers.TryGetValues("Server-Timing", out var values));
        var header = Assert.Single(values);
        Assert.StartsWith("app;dur=", header, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Data_protection_keys_are_persisted_in_database()
    {
        using var scope = _factory.Services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();
        var protector = provider.CreateProtector("integration-test");

        _ = protector.Protect("persistent-secret");

        var db = scope.ServiceProvider.GetRequiredService<DataProtectionDbContext>();
        Assert.True(await db.DataProtectionKeys.AnyAsync());
    }
}
