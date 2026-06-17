using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServicoPalavra.Application.Auth;
using ServicoPalavra.Application.Categorias;
using ServicoPalavra.Application.Conteudos;
using ServicoPalavra.Application.PlanosBiblicos;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;
using ServicoPalavra.Infrastructure.Persistence;

namespace ServicoPalavra.IntegrationTests;

public sealed class SecurityTests : IClassFixture<SecurityWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly SecurityWebApplicationFactory _factory;

    public SecurityTests(SecurityWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_valid_returns_cookie_without_password_hash_or_token()
    {
        await EnsureCsrfAsync();
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@tests.local", "Admin12"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("__Host-ServicoPalavra", response.Headers.GetValues("Set-Cookie").First());
        Assert.DoesNotContain("\"token\"", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("senhaHash", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_invalid_uses_generic_response()
    {
        await EnsureCsrfAsync();
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("missing@tests.local", "wrong"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Credenciais invalidas", body);
        Assert.DoesNotContain("missing@tests.local", body);
    }

    [Fact]
    public async Task Login_lockout_blocks_repeated_invalid_attempts()
    {
        await EnsureCsrfAsync();

        for (var i = 0; i < 5; i++)
        {
            var invalid = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@tests.local", "wrong-password"));
            Assert.Equal(HttpStatusCode.Unauthorized, invalid.StatusCode);
        }

        var locked = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@tests.local", "Admin12"));
        Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
    }

    [Fact]
    public async Task Protected_route_requires_authentication()
    {
        var response = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Admin_endpoint_denies_common_user_and_allows_admin()
    {
        await EnsureCsrfAsync();
        await RegisterAsync("Usuario A", "usuario-a@tests.local");

        var userResponse = await _client.PostAsJsonAsync("/api/admin/categorias", new CategoriaRequest("Formacao", null, null, null, null, 1));
        Assert.Equal(HttpStatusCode.Forbidden, userResponse.StatusCode);

        await LoginAsync("admin@tests.local", "Admin12");

        var adminResponse = await _client.PostAsJsonAsync("/api/admin/categorias", new CategoriaRequest("Formacao Admin", null, null, null, null, 1));
        Assert.Equal(HttpStatusCode.OK, adminResponse.StatusCode);

        var me = await _client.GetAsync("/api/auth/me");
        me.EnsureSuccessStatusCode();
        var body = await me.Content.ReadAsStringAsync();
        Assert.Contains("Admin", body);
    }

    [Fact]
    public async Task User_cannot_read_other_users_biblical_plan_even_with_guid()
    {
        await EnsureCsrfAsync();
        await SeedBibleBaseAsync();
        await RegisterAsync("Usuario Plano A", "plano-a@tests.local");
        await RegisterAsync("Usuario Plano B", "plano-b@tests.local");

        var createB = await _client.PostAsJsonAsync("/api/planos-biblicos", new CriarPlanoBiblicoRequest("Plano B", 0, 1, DateOnly.FromDateTime(DateTime.UtcNow)));
        createB.EnsureSuccessStatusCode();
        var planBId = await ReadGuidAsync(createB, "id");

        await LoginAsync("plano-a@tests.local", "User12");
        var crossRead = await _client.GetAsync($"/api/planos-biblicos/{planBId}");

        Assert.Equal(HttpStatusCode.NotFound, crossRead.StatusCode);
    }

    [Fact]
    public async Task Biblical_plan_generates_pastoral_order_continue_restart_and_history()
    {
        await SeedBibleBaseAsync();
        await RegisterAsync("Usuario Plano Pastoral", "plano-pastoral@tests.local");

        var create = await _client.PostAsJsonAsync("/api/planos-biblicos", new CriarPlanoBiblicoRequest("Plano Pastoral", 0, 1, DateOnly.FromDateTime(DateTime.UtcNow)));
        await EnsureSuccessAsync(create);
        var firstPlanId = await ReadGuidAsync(create, "id");

        var days = await _client.GetAsync($"/api/planos-biblicos/{firstPlanId}/dias");
        await EnsureSuccessAsync(days);
        using (var document = JsonDocument.Parse(await days.Content.ReadAsStringAsync()))
        {
            var items = document.RootElement.GetProperty("data").EnumerateArray().ToArray();
            Assert.Equal(30, items.Length);
            Assert.Contains("1 Joao 1", items[0].GetProperty("leiturasTexto").GetString());
            Assert.Contains("Meditação livre", items[^1].GetProperty("leiturasTexto").GetString());
            Assert.Contains("Tobias 1", items.First(x => x.GetProperty("leiturasTexto").GetString()?.Contains("Tobias 1") == true).GetProperty("leiturasTexto").GetString());
        }

        var firstDayId = await ReadFirstDayIdAsync(firstPlanId);
        var conclude = await _client.PostAsync($"/api/planos-biblicos/dias/{firstDayId}/concluir", null);
        await EnsureSuccessAsync(conclude);

        var continuePlan = await _client.PostAsJsonAsync("/api/planos-biblicos/alterar", new AlterarPlanoBiblicoRequest(0, 1, ModoAlteracaoPlanoBiblico.ContinuarDeOndeParei));
        await EnsureSuccessAsync(continuePlan);
        using (var document = JsonDocument.Parse(await continuePlan.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(2, data.GetProperty("ordemInicio").GetInt32());
            Assert.Equal("Continuacao", data.GetProperty("modoCriacao").GetString());
        }

        var restartPlan = await _client.PostAsJsonAsync("/api/planos-biblicos/alterar", new AlterarPlanoBiblicoRequest(0, 1, ModoAlteracaoPlanoBiblico.RecomecarDoInicio));
        await EnsureSuccessAsync(restartPlan);
        using (var document = JsonDocument.Parse(await restartPlan.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(1, data.GetProperty("ordemInicio").GetInt32());
            Assert.Equal("Reinicio", data.GetProperty("modoCriacao").GetString());
        }

        var history = await _client.GetAsync("/api/planos-biblicos/me/historico");
        await EnsureSuccessAsync(history);
        using (var document = JsonDocument.Parse(await history.Content.ReadAsStringAsync()))
        {
            Assert.Equal(3, document.RootElement.GetProperty("data").GetArrayLength());
        }
    }

    [Fact]
    public async Task User_cannot_conclude_other_users_day()
    {
        await EnsureCsrfAsync();
        await SeedBibleBaseAsync();
        await RegisterAsync("Usuario Dia A", "dia-a@tests.local");
        await RegisterAsync("Usuario Dia B", "dia-b@tests.local");

        var createB = await _client.PostAsJsonAsync("/api/planos-biblicos", new CriarPlanoBiblicoRequest("Plano B", 0, 1, DateOnly.FromDateTime(DateTime.UtcNow)));
        await EnsureSuccessAsync(createB);
        var planBId = await ReadGuidAsync(createB, "id");
        var dayBId = await ReadFirstDayIdAsync(planBId);

        await LoginAsync("dia-a@tests.local", "User12");
        var crossConclude = await _client.PostAsync($"/api/planos-biblicos/dias/{dayBId}/concluir", null);

        Assert.Equal(HttpStatusCode.NotFound, crossConclude.StatusCode);
    }

    [Fact]
    public async Task Biblical_plan_failed_continuation_keeps_active_plan()
    {
        await SeedBibleBaseAsync();
        await RegisterAsync("Usuario Plano Rollback", "plano-rollback@tests.local");

        var create = await _client.PostAsJsonAsync("/api/planos-biblicos", new CriarPlanoBiblicoRequest("Plano Rollback", 0, 1, DateOnly.FromDateTime(DateTime.UtcNow)));
        await EnsureSuccessAsync(create);
        var planId = await ReadGuidAsync(create, "id");

        var daysResponse = await _client.GetAsync($"/api/planos-biblicos/{planId}/dias");
        await EnsureSuccessAsync(daysResponse);
        using (var document = JsonDocument.Parse(await daysResponse.Content.ReadAsStringAsync()))
        {
            foreach (var day in document.RootElement.GetProperty("data").EnumerateArray())
            {
                var conclude = await _client.PostAsync($"/api/planos-biblicos/dias/{day.GetProperty("id").GetGuid()}/concluir", null);
                await EnsureSuccessAsync(conclude);
            }
        }

        var failed = await _client.PostAsJsonAsync("/api/planos-biblicos/alterar", new AlterarPlanoBiblicoRequest(0, 1, ModoAlteracaoPlanoBiblico.ContinuarDeOndeParei));
        Assert.Equal((HttpStatusCode)422, failed.StatusCode);

        var active = await _client.GetAsync("/api/planos-biblicos/me/ativo");
        await EnsureSuccessAsync(active);
        Assert.Equal(planId, await ReadGuidAsync(active, "id"));
    }

    [Fact]
    public async Task Biblical_plan_does_not_allow_two_active_plans_for_same_user()
    {
        await SeedBibleBaseAsync();
        await RegisterAsync("Usuario Plano Unico", "plano-unico@tests.local");

        var first = await _client.PostAsJsonAsync("/api/planos-biblicos", new CriarPlanoBiblicoRequest("Plano 1", 0, 1, DateOnly.FromDateTime(DateTime.UtcNow)));
        await EnsureSuccessAsync(first);

        var second = await _client.PostAsJsonAsync("/api/planos-biblicos", new CriarPlanoBiblicoRequest("Plano 2", 0, 1, DateOnly.FromDateTime(DateTime.UtcNow)));

        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    [Fact]
    public async Task Malicious_or_untrusted_content_url_is_rejected()
    {
        await EnsureCsrfAsync();
        await LoginAsync("admin@tests.local", "Admin12");

        var categoryResponse = await _client.PostAsJsonAsync("/api/admin/categorias", new CategoriaRequest("Videos", null, null, null, null, 1));
        await EnsureSuccessAsync(categoryResponse);
        var categoryId = await ReadGuidAsync(categoryResponse, "id");

        var request = new ConteudoRequest("Video ruim", null, null, null, TipoConteudo.Video, OrigemConteudo.YouTube, "javascript:alert(1)", null, null, categoryId, true, false, null);
        var response = await _client.PostAsJsonAsync("/api/admin/conteudos", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Error_response_does_not_expose_stack_trace()
    {
        await EnsureCsrfAsync();
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@tests.local", ""));
        var body = await response.Content.ReadAsStringAsync();

        Assert.DoesNotContain(" at ", body);
        Assert.DoesNotContain("ConnectionString", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", body, StringComparison.OrdinalIgnoreCase);
    }

    private async Task EnsureCsrfAsync()
    {
        var response = await _client.GetAsync("/api/auth/csrf");
        response.EnsureSuccessStatusCode();
        var token = await ReadStringAsync(response, "token");
        _client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        _client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token);
    }

    private async Task RegisterAsync(string nome, string email)
    {
        await EnsureCsrfAsync();
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(nome, email, "User12"));
        await EnsureSuccessAsync(response);
        await EnsureCsrfAsync();
    }

    private async Task LoginAsync(string email, string password)
    {
        await EnsureCsrfAsync();
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        await EnsureSuccessAsync(response);
        await EnsureCsrfAsync();
    }

    private async Task SeedBibleBaseAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.BaseBiblica.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;
        db.BaseBiblica.AddRange(
            TestChapter(1, "1 Joao", 1, "Cartas Joaninas", "Novo", 10, now),
            TestChapter(2, "1 Joao", 2, "Cartas Joaninas", "Novo", 29, now),
            TestChapter(3, "Joao", 1, "Evangelhos", "Novo", 51, now),
            TestChapter(4, "Romanos", 1, "Cartas Apostolicas", "Novo", 32, now),
            TestChapter(5, "Genesis", 1, "Pentateuco", "Antigo", 31, now),
            TestChapter(6, "Josue", 1, "Historicos", "Antigo", 18, now),
            TestChapter(7, "Isaias", 1, "Profetas", "Antigo", 31, now),
            TestChapter(8, "Salmos", 1, "Sapienciais", "Antigo", 6, now),
            TestChapter(9, "Tobias", 1, "Deuterocanonicos", "Antigo", 25, now));
        await db.SaveChangesAsync();
    }

    private static BaseBiblica TestChapter(int ordem, string livro, int capitulo, string grupo, string testamento, int peso, DateTime now) =>
        new()
        {
            Id = Guid.NewGuid(),
            Ordem = ordem,
            Livro = livro,
            Capitulo = capitulo,
            Grupo = grupo,
            Testamento = testamento,
            TempoEstimadoMinutos = Math.Max(2, (int)Math.Ceiling(peso / 4m)),
            QuantidadeVersiculos = peso,
            PesoLeitura = peso,
            CriadoEm = now
        };

    private async Task<Guid> ReadFirstDayIdAsync(Guid planId)
    {
        var days = await _client.GetAsync($"/api/planos-biblicos/{planId}/dias");
        await EnsureSuccessAsync(days);
        using var document = JsonDocument.Parse(await days.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").EnumerateArray().First().GetProperty("id").GetGuid();
    }

    private static async Task<Guid> ReadGuidAsync(HttpResponseMessage response, string property)
    {
        var value = await ReadStringAsync(response, property);
        return Guid.Parse(value);
    }

    private static async Task<string> ReadStringAsync(HttpResponseMessage response, string property)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty(property).GetString()!;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}). Body: {body}");
        }
    }
}
