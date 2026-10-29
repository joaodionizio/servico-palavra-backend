using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ServicoPalavra.Application.Auth;

namespace ServicoPalavra.IntegrationTests;

public sealed class DashboardPerformanceTests : IClassFixture<SecurityWebApplicationFactory>
{
    private readonly SecurityWebApplicationFactory _factory;

    public DashboardPerformanceTests(SecurityWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Dashboard_without_active_plan_uses_at_most_four_read_commands()
    {
        var client = _factory.CreateClient();
        await EnsureCsrfAsync(client);

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest("admin@tests.local", "Admin12"));
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        _factory.DashboardCommandCounter.Reset();

        var response = await client.GetAsync("/api/dashboard/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(
            _factory.DashboardCommandCounter.Count <= 4,
            $"Dashboard executou {_factory.DashboardCommandCounter.Count} comandos SQL de leitura; esperado no maximo 4 no cenario sem plano ativo.");
    }

    private static async Task EnsureCsrfAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var token = document.RootElement.GetProperty("data").GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token);
    }
}
