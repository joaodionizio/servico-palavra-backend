using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ServicoPalavra.Application.Auth;
using ServicoPalavra.Application.Conteudos;
using ServicoPalavra.Domain.Enums;
using ServicoPalavra.Infrastructure.Persistence;

namespace ServicoPalavra.IntegrationTests;

public sealed class ContentTransactionTests : IClassFixture<SecurityWebApplicationFactory>
{
    private readonly SecurityWebApplicationFactory _factory;

    public ContentTransactionTests(SecurityWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Failed_material_insert_rolls_back_deletion_and_content_update()
    {
        using var client = _factory.CreateClient();
        await EnsureCsrfAsync(client);
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("admin@tests.local", "Admin12"));
        login.EnsureSuccessStatusCode();

        var suffix = Guid.NewGuid().ToString("N");
        var original = new ConteudoRequest(
            Titulo: $"Conteudo original {suffix}",
            Slug: $"auditoria-transacao-{suffix}",
            Descricao: "Descricao original",
            Resumo: "Resumo",
            Tipo: TipoConteudo.Video,
            Origem: OrigemConteudo.YouTube,
            Url: "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
            UrlThumbnail: null,
            DuracaoMinutos: 5,
            CategoriaConteudoId: null,
            Publicado: true,
            Destaque: false,
            Ordem: 1,
            MateriaisApoio: [
                new MaterialApoioRequest("Roteiro original", null, TipoMaterialApoio.PDF, "https://example.com/original.pdf")
            ]);

        await EnsureCsrfAsync(client);
        var create = await client.PostAsJsonAsync("/api/admin/conteudos", original);
        create.EnsureSuccessStatusCode();
        using var createdJson = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        var id = createdJson.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.ExecuteSqlRawAsync(
                "CREATE TRIGGER audit_material_insert_failure BEFORE INSERT ON \"MateriaisApoio\" " +
                "BEGIN SELECT RAISE(ABORT, 'forced failure for rollback test'); END;");

            try
            {
                var updated = original with
                {
                    Titulo = $"Conteudo alterado {suffix}",
                    MateriaisApoio = [
                        new MaterialApoioRequest("Novo roteiro", null, TipoMaterialApoio.PDF, "https://example.com/new.pdf")
                    ]
                };

                await EnsureCsrfAsync(client);
                var update = await client.PutAsJsonAsync($"/api/admin/conteudos/{id}", updated);
                Assert.Equal(HttpStatusCode.InternalServerError, update.StatusCode);
            }
            finally
            {
                await db.Database.ExecuteSqlRawAsync("DROP TRIGGER IF EXISTS audit_material_insert_failure;");
            }
        }

        var detail = await client.GetAsync($"/api/admin/conteudos/{id}");
        detail.EnsureSuccessStatusCode();
        using var detailJson = JsonDocument.Parse(await detail.Content.ReadAsStringAsync());
        var data = detailJson.RootElement.GetProperty("data");
        Assert.Equal(original.Titulo, data.GetProperty("titulo").GetString());
        Assert.Equal(original.Descricao, data.GetProperty("descricao").GetString());
        var materials = data.GetProperty("materiaisApoio").EnumerateArray().ToArray();
        var material = Assert.Single(materials);
        Assert.Equal("Roteiro original", material.GetProperty("titulo").GetString());
        Assert.Equal("https://example.com/original.pdf", material.GetProperty("url").GetString());
    }

    private static async Task EnsureCsrfAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        response.EnsureSuccessStatusCode();
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var token = json.RootElement.GetProperty("data").GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token);
    }
}
