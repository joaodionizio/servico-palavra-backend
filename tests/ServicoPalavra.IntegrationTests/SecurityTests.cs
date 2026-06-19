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
using ServicoPalavra.Infrastructure.Persistence.Import;

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
    public async Task Authenticated_user_can_update_own_name()
    {
        await EnsureCsrfAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"perfil-nome-{suffix}@tests.local";
        await RegisterAsync("Usuario Perfil", email);

        var response = await _client.PutAsJsonAsync("/api/auth/me", new UpdateMeRequest("Usuario Perfil Atualizado", email));

        await EnsureSuccessAsync(response);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        Assert.Equal("Usuario Perfil Atualizado", data.GetProperty("nome").GetString());
        Assert.Equal(email, data.GetProperty("email").GetString());
        Assert.Contains(data.GetProperty("roles").EnumerateArray(), role => role.GetString() == "Usuario");
    }

    [Fact]
    public async Task Authenticated_user_can_update_email_and_identity_names()
    {
        await EnsureCsrfAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var originalEmail = $"perfil-email-{suffix}@tests.local";
        var newEmail = $"perfil-email-novo-{suffix}@tests.local";
        await RegisterAsync("Usuario Email", originalEmail);
        var userId = await ReadCurrentUserIdAsync();

        var response = await _client.PutAsJsonAsync("/api/auth/me", new UpdateMeRequest("Usuario Email", newEmail));

        await EnsureSuccessAsync(response);
        using (var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(newEmail, data.GetProperty("email").GetString());
            Assert.False(data.TryGetProperty("passwordHash", out _));
            Assert.False(data.TryGetProperty("normalizedEmail", out _));
        }

        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstAsync(x => x.Id == userId);
        Assert.Equal(newEmail, user.Email);
        Assert.Equal(newEmail, user.UserName);
        Assert.Equal(newEmail.ToUpperInvariant(), user.NormalizedEmail);
        Assert.Equal(newEmail.ToUpperInvariant(), user.NormalizedUserName);
    }

    [Fact]
    public async Task Update_me_rejects_duplicate_email()
    {
        await EnsureCsrfAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var firstEmail = $"perfil-duplicado-a-{suffix}@tests.local";
        var secondEmail = $"perfil-duplicado-b-{suffix}@tests.local";
        await RegisterAsync("Usuario Duplicado A", firstEmail);
        await RegisterAsync("Usuario Duplicado B", secondEmail);

        var response = await _client.PutAsJsonAsync("/api/auth/me", new UpdateMeRequest("Usuario Duplicado B", firstEmail));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_me_does_not_allow_role_changes()
    {
        await EnsureCsrfAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"perfil-role-{suffix}@tests.local";
        await RegisterAsync("Usuario Role", email);

        var response = await _client.PutAsJsonAsync("/api/auth/me", new
        {
            nome = "Usuario Role",
            email,
            roles = new[] { "Admin" }
        });

        await EnsureSuccessAsync(response);
        using (var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
        {
            var roles = document.RootElement.GetProperty("data").GetProperty("roles").EnumerateArray().Select(x => x.GetString()).ToArray();
            Assert.Contains("Usuario", roles);
            Assert.DoesNotContain("Admin", roles);
        }

        var adminOnly = await _client.GetAsync("/api/admin/categorias");
        Assert.Equal(HttpStatusCode.Forbidden, adminOnly.StatusCode);
    }

    [Fact]
    public async Task Update_me_requires_authentication()
    {
        await EnsureCsrfAsync();
        var response = await _client.PutAsJsonAsync("/api/auth/me", new UpdateMeRequest("Anonimo", "anonimo@tests.local"));
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
    public async Task Common_user_cannot_create_update_or_publish_admin_content()
    {
        await EnsureCsrfAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var contentId = await CreatePublishedContentAsync($"Conteudo Admin {suffix}");
        var categoryId = await CreateCategoryAsync($"Categoria Bloqueada {suffix}");

        await RegisterAsync("Usuario Admin Bloqueado", $"admin-blocked-{suffix}@tests.local");

        var createContent = await _client.PostAsJsonAsync("/api/admin/conteudos", BuildContentRequest($"Conteudo Bloqueado {suffix}", categoryId));
        Assert.Equal(HttpStatusCode.Forbidden, createContent.StatusCode);

        var updateContent = await _client.PutAsJsonAsync($"/api/admin/conteudos/{contentId}", BuildContentRequest($"Conteudo Alterado {suffix}", categoryId));
        Assert.Equal(HttpStatusCode.Forbidden, updateContent.StatusCode);

        var publishContent = await _client.PatchAsJsonAsync($"/api/admin/conteudos/{contentId}/publicacao", new ConteudoPublicacaoRequest(true));
        Assert.Equal(HttpStatusCode.Forbidden, publishContent.StatusCode);

        var deleteContent = await _client.DeleteAsync($"/api/admin/conteudos/{contentId}");
        Assert.Equal(HttpStatusCode.Forbidden, deleteContent.StatusCode);

        var unpublishContent = await _client.PatchAsync($"/api/admin/conteudos/{contentId}/despublicar", null);
        Assert.Equal(HttpStatusCode.Forbidden, unpublishContent.StatusCode);
    }

    [Fact]
    public async Task Public_categories_list_returns_only_active_ordered_contract_fields()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var slugPrefix = $"fase11a-{suffix}";
        var now = DateTime.UtcNow;

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.CategoriasConteudo.AddRange(
                new CategoriaConteudo
                {
                    Id = Guid.NewGuid(),
                    Nome = $"Zeta {suffix}",
                    Slug = $"{slugPrefix}-zeta",
                    Descricao = "Categoria ativa Zeta",
                    Cor = "#112233",
                    Icone = "book-open",
                    Ordem = 1,
                    Ativo = true,
                    CriadoEm = now
                },
                new CategoriaConteudo
                {
                    Id = Guid.NewGuid(),
                    Nome = $"Alpha {suffix}",
                    Slug = $"{slugPrefix}-alpha",
                    Descricao = "Categoria ativa Alpha",
                    Cor = "#445566",
                    Icone = "graduation-cap",
                    Ordem = 1,
                    Ativo = true,
                    CriadoEm = now
                },
                new CategoriaConteudo
                {
                    Id = Guid.NewGuid(),
                    Nome = $"Inativa {suffix}",
                    Slug = $"{slugPrefix}-inativa",
                    Ordem = 0,
                    Ativo = false,
                    CriadoEm = now
                });
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync("/api/categorias");

        await EnsureSuccessAsync(response);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var categories = data
            .EnumerateArray()
            .Where(x => x.GetProperty("slug").GetString()!.StartsWith(slugPrefix, StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(2, categories.Length);
        Assert.Equal($"{slugPrefix}-alpha", categories[0].GetProperty("slug").GetString());
        Assert.Equal($"{slugPrefix}-zeta", categories[1].GetProperty("slug").GetString());
        Assert.Equal($"Alpha {suffix}", categories[0].GetProperty("nome").GetString());
        Assert.Equal("Categoria ativa Alpha", categories[0].GetProperty("descricao").GetString());
        Assert.Equal("#445566", categories[0].GetProperty("cor").GetString());
        Assert.Equal("graduation-cap", categories[0].GetProperty("icone").GetString());
        Assert.Equal(1, categories[0].GetProperty("ordem").GetInt32());
        Assert.False(categories[0].TryGetProperty("ativo", out _));
        Assert.False(categories[0].TryGetProperty("criadoEm", out _));
        Assert.False(categories[0].TryGetProperty("atualizadoEm", out _));
    }

    [Fact]
    public async Task Common_user_cannot_create_update_deactivate_or_delete_admin_category()
    {
        await EnsureCsrfAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var categoryId = await CreateCategoryAsync($"Categoria Segura {suffix}");
        await RegisterAsync("Usuario Categoria Comum", $"categoria-comum-{suffix}@tests.local");

        var create = await _client.PostAsJsonAsync("/api/admin/categorias", new CategoriaRequest($"Categoria Bloqueada {suffix}", null, null, null, null, 1));
        Assert.Equal(HttpStatusCode.Forbidden, create.StatusCode);

        var update = await _client.PutAsJsonAsync($"/api/admin/categorias/{categoryId}", new CategoriaRequest($"Categoria Alterada {suffix}", null, null, null, null, 2));
        Assert.Equal(HttpStatusCode.Forbidden, update.StatusCode);

        var deactivate = await _client.PatchAsJsonAsync($"/api/admin/categorias/{categoryId}/status", new CategoriaStatusRequest(false));
        Assert.Equal(HttpStatusCode.Forbidden, deactivate.StatusCode);

        var delete = await _client.DeleteAsync($"/api/admin/categorias/{categoryId}");
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
    }

    [Fact]
    public async Task Admin_can_create_deactivate_list_inactive_and_public_hides_inactive_category()
    {
        await LoginAsync("admin@tests.local", "Admin12");
        var suffix = Guid.NewGuid().ToString("N");
        var create = await _client.PostAsJsonAsync("/api/admin/categorias", new CategoriaRequest($"Categoria Admin {suffix}", null, "Descricao admin", "#0A4F8F", "church", 3));
        await EnsureSuccessAsync(create);
        var categoryId = await ReadGuidAsync(create, "id");
        var slug = await ReadStringAsync(create, "slug");

        var publicActive = await _client.GetAsync("/api/categorias");
        await EnsureSuccessAsync(publicActive);
        Assert.Contains(categoryId, await ReadDataIdsAsync(publicActive));

        var deactivate = await _client.PatchAsJsonAsync($"/api/admin/categorias/{categoryId}/status", new CategoriaStatusRequest(false));
        await EnsureSuccessAsync(deactivate);
        using (var document = JsonDocument.Parse(await deactivate.Content.ReadAsStringAsync()))
        {
            Assert.False(document.RootElement.GetProperty("data").GetProperty("ativo").GetBoolean());
        }

        var publicInactive = await _client.GetAsync("/api/categorias");
        await EnsureSuccessAsync(publicInactive);
        Assert.DoesNotContain(categoryId, await ReadDataIdsAsync(publicInactive));

        var adminList = await _client.GetAsync("/api/admin/categorias");
        await EnsureSuccessAsync(adminList);
        using (var document = JsonDocument.Parse(await adminList.Content.ReadAsStringAsync()))
        {
            var category = document.RootElement.GetProperty("data").EnumerateArray().First(x => x.GetProperty("id").GetGuid() == categoryId);
            Assert.False(category.GetProperty("ativo").GetBoolean());
            Assert.Equal(slug, category.GetProperty("slug").GetString());
        }

        var adminDetail = await _client.GetAsync($"/api/admin/categorias/{categoryId}");
        await EnsureSuccessAsync(adminDetail);
        using (var document = JsonDocument.Parse(await adminDetail.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(categoryId, data.GetProperty("id").GetGuid());
            Assert.False(data.GetProperty("ativo").GetBoolean());
        }
    }

    [Fact]
    public async Task Public_filter_by_inactive_category_returns_empty_and_admin_delete_blocks_category_in_use()
    {
        await LoginAsync("admin@tests.local", "Admin12");
        var suffix = Guid.NewGuid().ToString("N");
        var content = await CreatePublishedContentWithDetailsAsync($"Categoria Em Uso {suffix}", TipoConteudo.Video, OrigemConteudo.YouTube, $"https://www.youtube.com/watch?v={Guid.NewGuid():N}");

        var deactivate = await _client.PatchAsJsonAsync($"/api/admin/categorias/{content.CategoryId}/status", new CategoriaStatusRequest(false));
        await EnsureSuccessAsync(deactivate);

        var publicFiltered = await _client.GetAsync($"/api/conteudos?categoriaSlug={content.CategorySlug}");
        await EnsureSuccessAsync(publicFiltered);
        using (var document = JsonDocument.Parse(await publicFiltered.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(0, data.GetProperty("totalItens").GetInt32());
            Assert.Empty(data.GetProperty("itens").EnumerateArray());
        }

        var deleteUsed = await _client.DeleteAsync($"/api/admin/categorias/{content.CategoryId}");
        Assert.Equal(HttpStatusCode.Conflict, deleteUsed.StatusCode);

        var unusedCategoryId = await CreateCategoryAsync($"Categoria Livre {suffix}");
        var deleteUnused = await _client.DeleteAsync($"/api/admin/categorias/{unusedCategoryId}");
        await EnsureSuccessAsync(deleteUnused);

        var deletedDetail = await _client.GetAsync($"/api/admin/categorias/{unusedCategoryId}");
        Assert.Equal(HttpStatusCode.NotFound, deletedDetail.StatusCode);
    }

    [Fact]
    public async Task Published_content_list_supports_search_category_type_and_pagination()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var first = await CreatePublishedContentWithDetailsAsync($"Formacao Audio {suffix}", TipoConteudo.Audio, OrigemConteudo.GoogleDrive, "https://drive.google.com/file/d/audio/view");
        _ = await CreatePublishedContentWithDetailsAsync($"Formacao Video {suffix}", TipoConteudo.Video, OrigemConteudo.YouTube, $"https://www.youtube.com/watch?v={Guid.NewGuid():N}");

        var response = await _client.GetAsync($"/api/conteudos?busca=Audio&categoriaSlug={first.CategorySlug}&tipo=Audio&pagina=1&tamanhoPagina=1");

        await EnsureSuccessAsync(response);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var items = data.GetProperty("itens").EnumerateArray().ToArray();

        Assert.Equal(1, data.GetProperty("pagina").GetInt32());
        Assert.Equal(1, data.GetProperty("tamanhoPagina").GetInt32());
        Assert.Equal(1, data.GetProperty("totalItens").GetInt32());
        Assert.Single(items);
        Assert.Equal(first.Id, items[0].GetProperty("id").GetGuid());
        Assert.Equal((int)TipoConteudo.Audio, items[0].GetProperty("tipo").GetInt32());
    }

    [Fact]
    public async Task Content_detail_by_slug_includes_support_material_and_authenticated_user_state()
    {
        await EnsureCsrfAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var content = await CreatePublishedContentWithDetailsAsync($"Detalhe {suffix}", TipoConteudo.Video, OrigemConteudo.YouTube, $"https://www.youtube.com/watch?v={Guid.NewGuid():N}", includeMaterial: true);
        var email = $"conteudo-detail-{suffix}@tests.local";

        var anonymous = await _client.GetAsync($"/api/conteudos/{content.Slug}");
        await EnsureSuccessAsync(anonymous);
        using (var document = JsonDocument.Parse(await anonymous.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(content.Id, data.GetProperty("id").GetGuid());
            Assert.False(data.GetProperty("favorito").GetBoolean());
            Assert.False(data.GetProperty("concluido").GetBoolean());
            Assert.Single(data.GetProperty("materiaisApoio").EnumerateArray());
        }

        await RegisterAsync("Usuario Detalhe Conteudo", email);
        await EnsureSuccessAsync(await _client.PostAsync($"/api/favoritos/{content.Id}", null));
        await EnsureSuccessAsync(await _client.PostAsync($"/api/progresso/conteudos/{content.Id}/concluir", null));

        var authenticated = await _client.GetAsync($"/api/conteudos/{content.Slug}");
        await EnsureSuccessAsync(authenticated);
        using (var document = JsonDocument.Parse(await authenticated.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.True(data.GetProperty("favorito").GetBoolean());
            Assert.True(data.GetProperty("concluido").GetBoolean());
        }

        await EnsureSuccessAsync(await _client.DeleteAsync($"/api/progresso/conteudos/{content.Id}/concluir"));
        var afterUncheck = await _client.GetAsync($"/api/conteudos/{content.Slug}");
        await EnsureSuccessAsync(afterUncheck);
        using (var document = JsonDocument.Parse(await afterUncheck.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.True(data.GetProperty("favorito").GetBoolean());
            Assert.False(data.GetProperty("concluido").GetBoolean());
        }
    }

    [Fact]
    public async Task Favorites_are_isolated_between_users()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var contentA = await CreatePublishedContentAsync($"Favorito A {suffix}");
        var contentB = await CreatePublishedContentAsync($"Favorito B {suffix}");
        var userAEmail = $"fav-a-{suffix}@tests.local";
        var userBEmail = $"fav-b-{suffix}@tests.local";

        await RegisterAsync("Usuario Favorito A", userAEmail);
        var favoriteA = await _client.PostAsync($"/api/conteudos/{contentA}/favoritar", null);
        await EnsureSuccessAsync(favoriteA);

        await RegisterAsync("Usuario Favorito B", userBEmail);
        var favoriteB = await _client.PostAsync($"/api/conteudos/{contentB}/favoritar", null);
        await EnsureSuccessAsync(favoriteB);

        await LoginAsync(userAEmail, "User12");
        var favoritesA = await _client.GetAsync("/api/favoritos");
        await EnsureSuccessAsync(favoritesA);
        var favoriteIdsA = await ReadDataIdsAsync(favoritesA);
        Assert.Contains(contentA, favoriteIdsA);
        Assert.DoesNotContain(contentB, favoriteIdsA);

        var removeA = await _client.DeleteAsync($"/api/conteudos/{contentA}/favoritar");
        await EnsureSuccessAsync(removeA);

        await LoginAsync(userBEmail, "User12");
        var favoritesBAfterRemoveA = await _client.GetAsync("/api/favoritos");
        await EnsureSuccessAsync(favoritesBAfterRemoveA);
        var favoriteIdsBAfterRemoveA = await ReadDataIdsAsync(favoritesBAfterRemoveA);
        Assert.Contains(contentB, favoriteIdsBAfterRemoveA);
        Assert.DoesNotContain(contentA, favoriteIdsBAfterRemoveA);
    }

    [Fact]
    public async Task Content_progress_is_isolated_between_users()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var contentA = await CreatePublishedContentAsync($"Progresso A {suffix}");
        var contentB = await CreatePublishedContentAsync($"Progresso B {suffix}");
        var userAEmail = $"progress-a-{suffix}@tests.local";
        var userBEmail = $"progress-b-{suffix}@tests.local";

        await RegisterAsync("Usuario Progresso A", userAEmail);
        await EnsureSuccessAsync(await _client.PostAsync($"/api/conteudos/{contentA}/favoritar", null));
        await EnsureSuccessAsync(await _client.PostAsync($"/api/conteudos/{contentA}/concluir", null));

        await RegisterAsync("Usuario Progresso B", userBEmail);
        await EnsureSuccessAsync(await _client.PostAsync($"/api/conteudos/{contentB}/favoritar", null));
        await EnsureSuccessAsync(await _client.PostAsync($"/api/conteudos/{contentB}/concluir", null));

        await LoginAsync(userAEmail, "User12");
        await EnsureSuccessAsync(await _client.PostAsync($"/api/conteudos/{contentA}/concluir", null));
        var dashboardA = await ReadDashboardAsync();
        Assert.Equal(1, dashboardA.ConteudosConcluidos);
        Assert.Equal(1, dashboardA.Favoritos);
        Assert.Contains(contentA, dashboardA.ConteudosComProgresso);
        Assert.DoesNotContain(contentB, dashboardA.ConteudosComProgresso);

        await LoginAsync(userBEmail, "User12");
        var dashboardB = await ReadDashboardAsync();
        Assert.Equal(1, dashboardB.ConteudosConcluidos);
        Assert.Equal(1, dashboardB.Favoritos);
        Assert.Contains(contentB, dashboardB.ConteudosComProgresso);
        Assert.DoesNotContain(contentA, dashboardB.ConteudosComProgresso);
    }

    [Fact]
    public async Task Dashboard_counts_only_authenticated_user_data()
    {
        await SeedBibleBaseAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var contentA = await CreatePublishedContentAsync($"Dashboard A {suffix}");
        var contentB1 = await CreatePublishedContentAsync($"Dashboard B1 {suffix}");
        var contentB2 = await CreatePublishedContentAsync($"Dashboard B2 {suffix}");
        var userAEmail = $"dash-a-{suffix}@tests.local";
        var userBEmail = $"dash-b-{suffix}@tests.local";

        await RegisterAsync("Usuario Dashboard A", userAEmail);
        await EnsureSuccessAsync(await _client.PostAsync($"/api/conteudos/{contentA}/favoritar", null));
        await EnsureSuccessAsync(await _client.PostAsync($"/api/conteudos/{contentA}/concluir", null));
        await EnsureSuccessAsync(await _client.PostAsJsonAsync("/api/planos-biblicos", new CriarPlanoBiblicoRequest("Plano Dashboard A", 0, 1, DateOnly.FromDateTime(DateTime.UtcNow))));

        await RegisterAsync("Usuario Dashboard B", userBEmail);
        await EnsureSuccessAsync(await _client.PostAsync($"/api/conteudos/{contentB1}/favoritar", null));
        await EnsureSuccessAsync(await _client.PostAsync($"/api/conteudos/{contentB2}/favoritar", null));
        await EnsureSuccessAsync(await _client.PostAsync($"/api/conteudos/{contentB1}/concluir", null));
        await EnsureSuccessAsync(await _client.PostAsJsonAsync("/api/planos-biblicos", new CriarPlanoBiblicoRequest("Plano Dashboard B", 0, 1, DateOnly.FromDateTime(DateTime.UtcNow))));

        await LoginAsync(userAEmail, "User12");
        var dashboardA = await ReadDashboardAsync();
        Assert.Equal(1, dashboardA.ConteudosConcluidos);
        Assert.Equal(1, dashboardA.Favoritos);
        Assert.True(dashboardA.PossuiPlanoBiblicoAtivo);
        Assert.Equal(1, dashboardA.TotalFavoritos);
        Assert.Equal(1, dashboardA.TotalConteudosConcluidos);
        Assert.Contains(contentA, dashboardA.FavoritosRecentes);
        Assert.Contains(contentA, dashboardA.ConteudosComProgresso);
        Assert.DoesNotContain(contentB1, dashboardA.FavoritosRecentes);
        Assert.DoesNotContain(contentB2, dashboardA.FavoritosRecentes);
        Assert.NotNull(dashboardA.PlanoBiblicoAtivoId);
        Assert.NotNull(dashboardA.LeituraHojeId);

        await LoginAsync(userBEmail, "User12");
        var dashboardB = await ReadDashboardAsync();
        Assert.Equal(1, dashboardB.ConteudosConcluidos);
        Assert.Equal(2, dashboardB.Favoritos);
        Assert.True(dashboardB.PossuiPlanoBiblicoAtivo);
        Assert.Equal(2, dashboardB.TotalFavoritos);
        Assert.Equal(1, dashboardB.TotalConteudosConcluidos);
        Assert.Contains(contentB1, dashboardB.FavoritosRecentes);
        Assert.Contains(contentB2, dashboardB.FavoritosRecentes);
        Assert.Contains(contentB1, dashboardB.ConteudosComProgresso);
        Assert.DoesNotContain(contentA, dashboardB.FavoritosRecentes);
        Assert.DoesNotContain(contentA, dashboardB.ConteudosComProgresso);
        Assert.NotNull(dashboardB.PlanoBiblicoAtivoId);
        Assert.NotNull(dashboardB.LeituraHojeId);
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
            Assert.Contains("1 João 1", items[0].GetProperty("leiturasTexto").GetString());
            Assert.Contains("Salmos 1", items[0].GetProperty("leiturasTexto").GetString());
            Assert.DoesNotContain("Gênesis", items[0].GetProperty("leiturasTexto").GetString());
        }

        var firstDayId = await ReadFirstDayIdAsync(firstPlanId);
        var conclude = await _client.PostAsync($"/api/planos-biblicos/dias/{firstDayId}/concluir", null);
        await EnsureSuccessAsync(conclude);

        var continuePlan = await _client.PostAsJsonAsync("/api/planos-biblicos/alterar", new AlterarPlanoBiblicoRequest(0, 1, ModoAlteracaoPlanoBiblico.ContinuarDeOndeParei));
        await EnsureSuccessAsync(continuePlan);
        using (var document = JsonDocument.Parse(await continuePlan.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.True(data.GetProperty("ordemInicio").GetInt32() > 1);
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
    public async Task User_can_conclude_unmark_and_conclude_biblical_plan_day_again()
    {
        await SeedBibleBaseAsync();
        await RegisterAsync("Usuario Toggle", "toggle@tests.local");

        var create = await _client.PostAsJsonAsync("/api/planos-biblicos", new CriarPlanoBiblicoRequest("Plano Toggle", 0, 1, DateOnly.FromDateTime(DateTime.UtcNow)));
        await EnsureSuccessAsync(create);
        var planId = await ReadGuidAsync(create, "id");
        var firstDayId = await ReadFirstDayIdAsync(planId);

        var conclude = await _client.PostAsync($"/api/planos-biblicos/dias/{firstDayId}/concluir", null);
        await EnsureSuccessAsync(conclude);
        Assert.True(await ReadDayConcluidoAsync(planId, firstDayId));
        Assert.True(await ReadCurrentOrderAsync() > 0);

        var unmark = await _client.PostAsync($"/api/planos-biblicos/dias/{firstDayId}/desmarcar", null);
        await EnsureSuccessAsync(unmark);
        Assert.False(await ReadDayConcluidoAsync(planId, firstDayId));
        Assert.Equal(0, await ReadCurrentOrderAsync());

        var concludeAgain = await _client.PostAsync($"/api/planos-biblicos/dias/{firstDayId}/concluir", null);
        await EnsureSuccessAsync(concludeAgain);
        Assert.True(await ReadDayConcluidoAsync(planId, firstDayId));
        Assert.True(await ReadCurrentOrderAsync() > 0);
    }

    [Fact]
    public async Task User_cannot_unmark_other_users_day()
    {
        await EnsureCsrfAsync();
        await SeedBibleBaseAsync();
        await RegisterAsync("Usuario Desmarcar A", "desmarcar-a@tests.local");
        await RegisterAsync("Usuario Desmarcar B", "desmarcar-b@tests.local");

        var createB = await _client.PostAsJsonAsync("/api/planos-biblicos", new CriarPlanoBiblicoRequest("Plano B", 0, 1, DateOnly.FromDateTime(DateTime.UtcNow)));
        await EnsureSuccessAsync(createB);
        var planBId = await ReadGuidAsync(createB, "id");
        var dayBId = await ReadFirstDayIdAsync(planBId);

        await LoginAsync("desmarcar-a@tests.local", "User12");
        var crossUnmark = await _client.PostAsync($"/api/planos-biblicos/dias/{dayBId}/desmarcar", null);

        Assert.Equal(HttpStatusCode.NotFound, crossUnmark.StatusCode);
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
    public async Task Admin_can_create_edit_publish_and_public_content_visibility_follows_publication_state()
    {
        await LoginAsync("admin@tests.local", "Admin12");
        var suffix = Guid.NewGuid().ToString("N");
        var categoryId = await CreateCategoryAsync($"Categoria Admin Conteudo {suffix}");
        var createRequest = new ConteudoRequest(
            $"Formacao Admin {suffix}",
            null,
            "Descricao inicial",
            "Resumo inicial",
            TipoConteudo.Video,
            OrigemConteudo.YouTube,
            $"https://www.youtube.com/watch?v={Guid.NewGuid():N}",
            "https://example.com/thumb.jpg",
            45,
            categoryId,
            false,
            false,
            0,
            [
                new MaterialApoioRequest("Roteiro", "Roteiro inicial", TipoMaterialApoio.PDF, "https://example.com/roteiro.pdf", 1, true)
            ]);

        var create = await _client.PostAsJsonAsync("/api/admin/conteudos", createRequest);
        await EnsureSuccessAsync(create);
        var contentId = await ReadGuidAsync(create, "id");
        var slug = await ReadStringAsync(create, "slug");

        var publicDraft = await _client.GetAsync($"/api/conteudos/{slug}");
        Assert.Equal(HttpStatusCode.NotFound, publicDraft.StatusCode);

        var adminDraftList = await _client.GetAsync($"/api/admin/conteudos?busca={Uri.EscapeDataString(suffix)}&publicado=false&pagina=1&tamanhoPagina=20");
        await EnsureSuccessAsync(adminDraftList);
        using (var document = JsonDocument.Parse(await adminDraftList.Content.ReadAsStringAsync()))
        {
            var items = document.RootElement.GetProperty("data").GetProperty("itens").EnumerateArray().ToArray();
            Assert.Contains(items, x => x.GetProperty("id").GetGuid() == contentId && !x.GetProperty("publicado").GetBoolean());
        }

        var updateRequest = createRequest with
        {
            Titulo = $"Formacao Admin Editada {suffix}",
            Slug = slug,
            Resumo = "Resumo editado",
            MateriaisApoio =
            [
                new MaterialApoioRequest("Roteiro editado", "Roteiro final", TipoMaterialApoio.Link, "https://example.com/roteiro-final", 2, false)
            ]
        };
        var update = await _client.PutAsJsonAsync($"/api/admin/conteudos/{contentId}", updateRequest);
        await EnsureSuccessAsync(update);

        var adminDetail = await _client.GetAsync($"/api/admin/conteudos/{contentId}");
        await EnsureSuccessAsync(adminDetail);
        using (var document = JsonDocument.Parse(await adminDetail.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal($"Formacao Admin Editada {suffix}", data.GetProperty("titulo").GetString());
            Assert.False(data.GetProperty("publicado").GetBoolean());
            var material = Assert.Single(data.GetProperty("materiaisApoio").EnumerateArray());
            Assert.Equal("Roteiro editado", material.GetProperty("titulo").GetString());
            Assert.False(material.GetProperty("ativo").GetBoolean());
        }

        var publish = await _client.PatchAsJsonAsync($"/api/admin/conteudos/{contentId}/publicacao", new ConteudoPublicacaoRequest(true));
        await EnsureSuccessAsync(publish);

        var publicPublished = await _client.GetAsync($"/api/conteudos/{slug}");
        await EnsureSuccessAsync(publicPublished);
        using (var document = JsonDocument.Parse(await publicPublished.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(contentId, data.GetProperty("id").GetGuid());
            Assert.True(data.GetProperty("publicado").GetBoolean());
            Assert.Empty(data.GetProperty("materiaisApoio").EnumerateArray());
        }
    }

    [Fact]
    public async Task Admin_can_create_content_without_category()
    {
        await LoginAsync("admin@tests.local", "Admin12");
        var suffix = Guid.NewGuid().ToString("N");
        var request = new ConteudoRequest(
            $"Formacao Sem Categoria {suffix}",
            null,
            null,
            null,
            TipoConteudo.Video,
            OrigemConteudo.YouTube,
            $"https://www.youtube.com/watch?v={Guid.NewGuid():N}",
            null,
            null,
            null,
            true,
            false,
            null);

        var create = await _client.PostAsJsonAsync("/api/admin/conteudos", request);
        await EnsureSuccessAsync(create);
        var contentId = await ReadGuidAsync(create, "id");
        var slug = await ReadStringAsync(create, "slug");

        using (var document = JsonDocument.Parse(await create.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(JsonValueKind.Null, data.GetProperty("categoriaConteudoId").ValueKind);
            Assert.Equal(JsonValueKind.Null, data.GetProperty("categoria").ValueKind);
        }

        var adminDetail = await _client.GetAsync($"/api/admin/conteudos/{contentId}");
        await EnsureSuccessAsync(adminDetail);
        using (var document = JsonDocument.Parse(await adminDetail.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(JsonValueKind.Null, data.GetProperty("categoriaConteudoId").ValueKind);
            Assert.Equal(JsonValueKind.Null, data.GetProperty("categoria").ValueKind);
        }

        var publicDetail = await _client.GetAsync($"/api/conteudos/{slug}");
        await EnsureSuccessAsync(publicDetail);
        using (var document = JsonDocument.Parse(await publicDetail.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.Equal(contentId, data.GetProperty("id").GetGuid());
            Assert.Equal(JsonValueKind.Null, data.GetProperty("categoriaConteudoId").ValueKind);
            Assert.Equal(JsonValueKind.Null, data.GetProperty("categoria").ValueKind);
        }
    }

    [Fact]
    public async Task Admin_create_defaults_to_published_and_support_material_defaults()
    {
        await LoginAsync("admin@tests.local", "Admin12");
        var suffix = Guid.NewGuid().ToString("N");
        var request = new
        {
            titulo = $"Formacao Defaults {suffix}",
            tipo = TipoConteudo.Video,
            origem = OrigemConteudo.YouTube,
            url = $"https://www.youtube.com/watch?v={Guid.NewGuid():N}",
            materiaisApoio = new[]
            {
                new
                {
                    titulo = "Roteiro externo",
                    tipo = TipoMaterialApoio.PDF,
                    url = "https://example.com/roteiro.pdf"
                }
            }
        };

        var create = await _client.PostAsJsonAsync("/api/admin/conteudos", request);
        await EnsureSuccessAsync(create);
        var contentId = await ReadGuidAsync(create, "id");
        var slug = await ReadStringAsync(create, "slug");

        using (var document = JsonDocument.Parse(await create.Content.ReadAsStringAsync()))
        {
            var data = document.RootElement.GetProperty("data");
            Assert.True(data.GetProperty("publicado").GetBoolean());
        }

        var publicDetail = await _client.GetAsync($"/api/conteudos/{slug}");
        await EnsureSuccessAsync(publicDetail);

        var adminDetail = await _client.GetAsync($"/api/admin/conteudos/{contentId}");
        await EnsureSuccessAsync(adminDetail);
        using (var document = JsonDocument.Parse(await adminDetail.Content.ReadAsStringAsync()))
        {
            var material = Assert.Single(document.RootElement.GetProperty("data").GetProperty("materiaisApoio").EnumerateArray());
            Assert.Equal(1, material.GetProperty("ordem").GetInt32());
            Assert.True(material.GetProperty("ativo").GetBoolean());
        }
    }

    [Fact]
    public async Task Admin_can_delete_content_and_remove_it_from_public_and_admin_lists()
    {
        await EnsureCsrfAsync();
        var suffix = Guid.NewGuid().ToString("N");
        var content = await CreatePublishedContentWithDetailsAsync($"Excluir {suffix}", TipoConteudo.Video, OrigemConteudo.YouTube, $"https://www.youtube.com/watch?v={Guid.NewGuid():N}", includeMaterial: true);
        var userEmail = $"delete-content-{suffix}@tests.local";

        await RegisterAsync("Usuario Delete Conteudo", userEmail);
        await EnsureSuccessAsync(await _client.PostAsync($"/api/favoritos/{content.Id}", null));
        await EnsureSuccessAsync(await _client.PostAsync($"/api/progresso/conteudos/{content.Id}/concluir", null));

        await LoginAsync("admin@tests.local", "Admin12");
        var delete = await _client.DeleteAsync($"/api/admin/conteudos/{content.Id}");
        await EnsureSuccessAsync(delete);

        var publicDetail = await _client.GetAsync($"/api/conteudos/{content.Slug}");
        Assert.Equal(HttpStatusCode.NotFound, publicDetail.StatusCode);

        var adminDetail = await _client.GetAsync($"/api/admin/conteudos/{content.Id}");
        Assert.Equal(HttpStatusCode.NotFound, adminDetail.StatusCode);

        var adminList = await _client.GetAsync($"/api/admin/conteudos?busca={Uri.EscapeDataString(suffix)}");
        await EnsureSuccessAsync(adminList);
        using (var document = JsonDocument.Parse(await adminList.Content.ReadAsStringAsync()))
        {
            var items = document.RootElement.GetProperty("data").GetProperty("itens").EnumerateArray().ToArray();
            Assert.DoesNotContain(items, x => x.GetProperty("id").GetGuid() == content.Id);
        }
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
        if (email == "admin@tests.local")
        {
            await ResetLoginStateAsync(email);
        }

        await EnsureCsrfAsync();
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        await EnsureSuccessAsync(response);
        await EnsureCsrfAsync();
    }

    private async Task<Guid> ReadCurrentUserIdAsync()
    {
        var response = await _client.GetAsync("/api/auth/me");
        await EnsureSuccessAsync(response);
        return await ReadGuidAsync(response, "id");
    }

    private async Task ResetLoginStateAsync(string email)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = await db.Users.FirstOrDefaultAsync(x => x.Email == email);

        if (user is null)
        {
            return;
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        await db.SaveChangesAsync();
    }

    private async Task SeedBibleBaseAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (db.BaseBiblica.Any())
        {
            return;
        }

        var importer = new BaseBiblicaV2Importer(db);
        await importer.ImportAsync(FindRepositoryFile("docs/examples/base-biblica-capitulos-com-versiculos.v2.json"));
    }

    private async Task<Guid> ReadFirstDayIdAsync(Guid planId)
    {
        var days = await _client.GetAsync($"/api/planos-biblicos/{planId}/dias");
        await EnsureSuccessAsync(days);
        using var document = JsonDocument.Parse(await days.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").EnumerateArray().First().GetProperty("id").GetGuid();
    }

    private async Task<bool> ReadDayConcluidoAsync(Guid planId, Guid dayId)
    {
        var days = await _client.GetAsync($"/api/planos-biblicos/{planId}/dias");
        await EnsureSuccessAsync(days);
        using var document = JsonDocument.Parse(await days.Content.ReadAsStringAsync());
        return document.RootElement
            .GetProperty("data")
            .EnumerateArray()
            .First(x => x.GetProperty("id").GetGuid() == dayId)
            .GetProperty("concluido")
            .GetBoolean();
    }

    private async Task<int> ReadCurrentOrderAsync()
    {
        var response = await _client.GetAsync("/api/planos-biblicos/progresso/posicao-atual");
        await EnsureSuccessAsync(response);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("data").GetProperty("ultimaOrdemConcluida").GetInt32();
    }

    private async Task<Guid> CreatePublishedContentAsync(string title)
    {
        var categoryId = await CreateCategoryAsync($"Categoria {title}");
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var admin = await db.Users.FirstAsync(x => x.Email == "admin@tests.local");
        var now = DateTime.UtcNow;
        var content = new Conteudo
        {
            Id = Guid.NewGuid(),
            Titulo = title,
            Slug = $"conteudo-{Guid.NewGuid():N}",
            Tipo = TipoConteudo.Video,
            Origem = OrigemConteudo.YouTube,
            Url = $"https://www.youtube.com/watch?v={Guid.NewGuid():N}",
            DuracaoMinutos = 10,
            CategoriaConteudoId = categoryId,
            CriadoPorUsuarioId = admin.Id,
            Publicado = true,
            PublicadoEm = now,
            CriadoEm = now
        };

        db.Conteudos.Add(content);
        await db.SaveChangesAsync();
        return content.Id;
    }

    private async Task<ContentSeed> CreatePublishedContentWithDetailsAsync(string title, TipoConteudo tipo, OrigemConteudo origem, string url, bool includeMaterial = false)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var admin = await db.Users.FirstAsync(x => x.Email == "admin@tests.local");
        var now = DateTime.UtcNow;
        var category = new CategoriaConteudo
        {
            Id = Guid.NewGuid(),
            Nome = $"Categoria {title}",
            Slug = $"categoria-{Guid.NewGuid():N}",
            Ativo = true,
            Ordem = 1,
            CriadoEm = now
        };
        var content = new Conteudo
        {
            Id = Guid.NewGuid(),
            Titulo = title,
            Slug = $"conteudo-{Guid.NewGuid():N}",
            Resumo = $"Resumo {title}",
            Descricao = $"Descricao {title}",
            Tipo = tipo,
            Origem = origem,
            Url = url,
            DuracaoMinutos = 10,
            CategoriaConteudoId = category.Id,
            CriadoPorUsuarioId = admin.Id,
            Publicado = true,
            PublicadoEm = now,
            CriadoEm = now
        };

        db.CategoriasConteudo.Add(category);
        db.Conteudos.Add(content);

        if (includeMaterial)
        {
            db.MateriaisApoio.Add(new MaterialApoio
            {
                Id = Guid.NewGuid(),
                ConteudoId = content.Id,
                Titulo = $"Material {title}",
                Tipo = TipoMaterialApoio.Link,
                Url = "https://drive.google.com/file/d/material/view",
                Ordem = 1,
                Ativo = true,
                CriadoEm = now
            });
        }

        await db.SaveChangesAsync();
        return new ContentSeed(content.Id, content.Slug, category.Id, category.Slug);
    }

    private async Task<Guid> CreateCategoryAsync(string name)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var category = new CategoriaConteudo
        {
            Id = Guid.NewGuid(),
            Nome = name,
            Slug = $"categoria-{Guid.NewGuid():N}",
            Ativo = true,
            Ordem = 1,
            CriadoEm = DateTime.UtcNow
        };

        db.CategoriasConteudo.Add(category);
        await db.SaveChangesAsync();
        return category.Id;
    }

    private static ConteudoRequest BuildContentRequest(string title, Guid categoryId) =>
        new(title, null, null, null, TipoConteudo.Video, OrigemConteudo.YouTube, $"https://www.youtube.com/watch?v={Guid.NewGuid():N}", null, 10, categoryId, true, false, 1);

    private async Task<IReadOnlyCollection<Guid>> ReadDataIdsAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement
            .GetProperty("data")
            .EnumerateArray()
            .Select(x => x.GetProperty("id").GetGuid())
            .ToArray();
    }

    private async Task<DashboardSnapshot> ReadDashboardAsync()
    {
        var response = await _client.GetAsync("/api/dashboard/me");
        await EnsureSuccessAsync(response);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = document.RootElement.GetProperty("data");
        var estatisticas = data.GetProperty("estatisticas");
        var planoBiblicoAtivo = data.GetProperty("planoBiblicoAtivo");
        var leituraHoje = data.GetProperty("leituraHoje");

        return new DashboardSnapshot(
            data.GetProperty("conteudosConcluidos").GetInt32(),
            data.GetProperty("favoritos").GetInt32(),
            data.GetProperty("possuiPlanoBiblicoAtivo").GetBoolean(),
            data.GetProperty("favoritosRecentes").EnumerateArray().Select(x => x.GetProperty("conteudoId").GetGuid()).ToArray(),
            data.GetProperty("conteudosComProgresso").EnumerateArray().Select(x => x.GetProperty("conteudoId").GetGuid()).ToArray(),
            estatisticas.GetProperty("totalFavoritos").GetInt32(),
            estatisticas.GetProperty("totalConteudosConcluidos").GetInt32(),
            planoBiblicoAtivo.ValueKind == JsonValueKind.Null ? null : planoBiblicoAtivo.GetProperty("id").GetGuid(),
            leituraHoje.ValueKind == JsonValueKind.Null ? null : leituraHoje.GetProperty("diaId").GetGuid());
    }

    private sealed record DashboardSnapshot(
        int ConteudosConcluidos,
        int Favoritos,
        bool PossuiPlanoBiblicoAtivo,
        IReadOnlyCollection<Guid> FavoritosRecentes,
        IReadOnlyCollection<Guid> ConteudosComProgresso,
        int TotalFavoritos,
        int TotalConteudosConcluidos,
        Guid? PlanoBiblicoAtivoId,
        Guid? LeituraHojeId);

    private sealed record ContentSeed(Guid Id, string Slug, Guid CategoryId, string CategorySlug);

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

    private static string FindRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Arquivo nao encontrado no repositorio: {relativePath}");
    }
}
