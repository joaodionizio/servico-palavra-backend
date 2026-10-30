using Microsoft.AspNetCore.DataProtection;
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
    public void Sqlite_test_environment_keeps_data_protection_available_without_production_store()
    {
        using var scope = _factory.Services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IDataProtectionProvider>();

        var protector = provider.CreateProtector("integration-test");
        var protectedValue = protector.Protect("secret");

        Assert.Equal("secret", protector.Unprotect(protectedValue));
        Assert.Null(scope.ServiceProvider.GetService<DataProtectionDbContext>());
    }
}
