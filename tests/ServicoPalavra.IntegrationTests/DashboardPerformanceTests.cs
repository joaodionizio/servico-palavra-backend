using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ServicoPalavra.Application.Auth;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;
using ServicoPalavra.Infrastructure.Persistence;

namespace ServicoPalavra.IntegrationTests;

public sealed class DashboardPerformanceTests : IClassFixture<SecurityWebApplicationFactory>
{
    private readonly SecurityWebApplicationFactory _factory;

    public DashboardPerformanceTests(SecurityWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Dashboard_without_active_plan_uses_exactly_four_read_commands()
    {
        var (client, _) = await RegisterUserAsync();

        _factory.DashboardCommandCounter.Reset();

        var response = await client.GetAsync("/api/dashboard/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(4, _factory.DashboardCommandCounter.Count);
    }

    [Fact]
    public async Task Dashboard_with_active_plan_and_today_reading_uses_exactly_five_read_commands()
    {
        var (client, userId) = await RegisterUserAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var planId = Guid.NewGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.PlanosBiblicosUsuario.Add(new PlanoBiblicoUsuario
            {
                Id = planId,
                UsuarioId = userId,
                Nome = "Plano performance",
                DuracaoDias = 1,
                DataInicio = today,
                DataFimPrevista = today,
                ModoCriacao = ModoCriacaoPlanoBiblico.NovoDoInicio,
                Ativo = true,
                Status = StatusPlanoBiblico.Ativo,
                CriadoEm = DateTime.UtcNow
            });
            db.PlanosBiblicosDias.Add(new PlanoBiblicoDia
            {
                Id = Guid.NewGuid(),
                PlanoBiblicoUsuarioId = planId,
                DiaNumero = 1,
                MesNumero = 1,
                DataPrevista = today,
                LeiturasTexto = "Evangelho de Joao 1",
                CriadoEm = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        _factory.DashboardCommandCounter.Reset();

        var response = await client.GetAsync("/api/dashboard/me");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(5, _factory.DashboardCommandCounter.Count);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal(planId, data.GetProperty("planoBiblicoAtivo").GetProperty("id").GetGuid());
        Assert.Equal(1, data.GetProperty("leituraHoje").GetProperty("diaNumero").GetInt32());
    }

    private async Task<(HttpClient Client, Guid UserId)> RegisterUserAsync()
    {
        var client = _factory.CreateClient();
        await EnsureCsrfAsync(client);
        var email = $"dashboard-{Guid.NewGuid():N}@tests.local";

        var response = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("Dashboard Performance", email, "StrongPass123"));
        response.EnsureSuccessStatusCode();

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var userId = document.RootElement
            .GetProperty("data")
            .GetProperty("usuario")
            .GetProperty("id")
            .GetGuid();

        return (client, userId);
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
