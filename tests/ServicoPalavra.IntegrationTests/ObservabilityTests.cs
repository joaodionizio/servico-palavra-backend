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
}
