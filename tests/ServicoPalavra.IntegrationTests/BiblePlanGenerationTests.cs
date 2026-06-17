using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Application.PlanosBiblicos;
using ServicoPalavra.Domain.Enums;
using ServicoPalavra.Infrastructure.Persistence;
using ServicoPalavra.Infrastructure.Persistence.Import;

namespace ServicoPalavra.IntegrationTests;

public sealed class BiblePlanGenerationTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"servico-palavra-plan-generation-{Guid.NewGuid():N}.db");

    [Fact]
    public async Task GenerateAsync_with_real_base_uses_pastoral_sequence_and_preserves_reading_weight()
    {
        await using var db = await CreateDbWithRealBaseAsync();
        var generator = CreateGenerator(db);

        var plan = await generator.GenerateAsync(
            Guid.NewGuid(),
            "Plano real",
            duracaoAnos: 0,
            duracaoMeses: 6,
            DateOnly.FromDateTime(DateTime.UtcNow),
            ModoCriacaoPlanoBiblico.NovoDoInicio,
            ordemInicio: 1,
            planoOrigemId: null);

        var generatedChapters = plan.Dias.SelectMany(x => x.Capitulos).OrderBy(x => x.Ordem).ToArray();
        var orders = generatedChapters.Select(x => x.Ordem).ToArray();
        var weights = generatedChapters.Sum(x => x.BaseBiblica.PesoLeitura);

        var days = plan.Dias.OrderBy(x => x.DiaNumero).ToArray();

        Assert.Equal(180, days.Length);
        Assert.Equal("1 João", generatedChapters[0].BaseBiblica.Livro);
        Assert.NotEqual("Gênesis", generatedChapters[0].BaseBiblica.Livro);
        Assert.Equal(1215, generatedChapters.Length);
        Assert.Equal(Enumerable.Range(1, 1215), orders);
        Assert.Equal(orders.Length, orders.Distinct().Count());
        Assert.True(weights > 0);
        Assert.Contains("Salmos 1", days[0].LeiturasTexto);
    }

    [Fact]
    public async Task GenerateAsync_repeats_joannine_books_as_pastoral_sequence_requires()
    {
        await using var db = await CreateDbWithRealBaseAsync();
        var generator = CreateGenerator(db);

        var plan = await generator.GenerateAsync(
            Guid.NewGuid(),
            "Plano real",
            duracaoAnos: 0,
            duracaoMeses: 6,
            DateOnly.FromDateTime(DateTime.UtcNow),
            ModoCriacaoPlanoBiblico.NovoDoInicio,
            ordemInicio: 1,
            planoOrigemId: null);

        var generatedChapters = plan.Dias.SelectMany(x => x.Capitulos).OrderBy(x => x.Ordem).ToArray();

        Assert.Equal(15, generatedChapters.Count(x => x.BaseBiblica.Livro == "1 João"));
        Assert.Equal(3, generatedChapters.Count(x => x.BaseBiblica.Livro == "1 João" && x.BaseBiblica.Capitulo == 1));
        Assert.Equal(42, generatedChapters.Count(x => x.BaseBiblica.Livro == "João"));
        Assert.Equal(2, generatedChapters.Count(x => x.BaseBiblica.Livro == "João" && x.BaseBiblica.Capitulo == 1));
    }

    [Fact]
    public async Task GenerateAsync_distributes_psalms_as_parallel_daily_reading()
    {
        await using var db = await CreateDbWithRealBaseAsync();
        var generator = CreateGenerator(db);

        var plan = await generator.GenerateAsync(
            Guid.NewGuid(),
            "Plano com salmos",
            duracaoAnos: 0,
            duracaoMeses: 6,
            DateOnly.FromDateTime(DateTime.UtcNow),
            ModoCriacaoPlanoBiblico.NovoDoInicio,
            ordemInicio: 1,
            planoOrigemId: null);

        var days = plan.Dias.OrderBy(x => x.DiaNumero).ToArray();

        Assert.Contains("Salmos 1", days[0].LeiturasTexto);
        Assert.Contains("Salmos 2", days[1].LeiturasTexto);
        Assert.Contains("Salmos 150", days[149].LeiturasTexto);
        Assert.DoesNotContain("Salmos", days[150].LeiturasTexto);
    }

    [Fact]
    public async Task GenerateAsync_respects_isaiah_pastoral_blocks()
    {
        await using var db = await CreateDbWithRealBaseAsync();
        var generator = CreateGenerator(db);

        var plan = await generator.GenerateAsync(
            Guid.NewGuid(),
            "Plano com Isaias",
            duracaoAnos: 0,
            duracaoMeses: 6,
            DateOnly.FromDateTime(DateTime.UtcNow),
            ModoCriacaoPlanoBiblico.NovoDoInicio,
            ordemInicio: 1,
            planoOrigemId: null);

        var generatedChapters = plan.Dias.SelectMany(x => x.Capitulos).OrderBy(x => x.Ordem).ToArray();
        var firstBlockEnd = Array.FindLastIndex(generatedChapters, x => x.BaseBiblica.Livro == "Isaías" && x.BaseBiblica.Capitulo <= 39);
        var secondBlockStart = Array.FindIndex(generatedChapters, x => x.BaseBiblica.Livro == "Isaías" && x.BaseBiblica.Capitulo == 40);
        var secondBlockEnd = Array.FindLastIndex(generatedChapters, x => x.BaseBiblica.Livro == "Isaías" && x.BaseBiblica.Capitulo <= 55);
        var thirdBlockStart = Array.FindIndex(generatedChapters, x => x.BaseBiblica.Livro == "Isaías" && x.BaseBiblica.Capitulo == 56);

        Assert.True(firstBlockEnd > 0);
        Assert.True(secondBlockStart > firstBlockEnd);
        Assert.True(thirdBlockStart > secondBlockEnd);
        Assert.Equal(66, generatedChapters.Last(x => x.BaseBiblica.Livro == "Isaías").BaseBiblica.Capitulo);
    }

    [Fact]
    public async Task GenerateAsync_generates_only_existing_base_chapters()
    {
        await using var db = await CreateDbWithRealBaseAsync();
        var generator = CreateGenerator(db);

        var plan = await generator.GenerateAsync(
            Guid.NewGuid(),
            "Plano real",
            duracaoAnos: 0,
            duracaoMeses: 6,
            DateOnly.FromDateTime(DateTime.UtcNow),
            ModoCriacaoPlanoBiblico.NovoDoInicio,
            ordemInicio: 1,
            planoOrigemId: null);

        var baseIds = await db.BaseBiblica.Select(x => x.Id).ToListAsync();
        var generatedBaseIds = plan.Dias.SelectMany(x => x.Capitulos).Select(x => x.BaseBiblicaId).ToArray();

        Assert.All(generatedBaseIds, id => Assert.Contains(id, baseIds));
    }

    [Theory]
    [InlineData(0, 6, 180)]
    [InlineData(1, 0, 365)]
    [InlineData(2, 0, 730)]
    public async Task GenerateAsync_creates_expected_day_count_for_common_durations(int years, int months, int expectedDays)
    {
        await using var db = await CreateDbWithRealBaseAsync();
        var generator = CreateGenerator(db);

        var plan = await generator.GenerateAsync(
            Guid.NewGuid(),
            "Plano por duracao",
            years,
            months,
            DateOnly.FromDateTime(DateTime.UtcNow),
            ModoCriacaoPlanoBiblico.NovoDoInicio,
            ordemInicio: 1,
            planoOrigemId: null);

        Assert.Equal(expectedDays, plan.DuracaoDias);
        Assert.Equal(expectedDays, plan.Dias.Count);
    }

    [Fact]
    public async Task GenerateAsync_continue_starts_after_last_completed_order()
    {
        await using var db = await CreateDbWithRealBaseAsync();
        var generator = CreateGenerator(db);

        var plan = await generator.GenerateAsync(
            Guid.NewGuid(),
            "Continuar",
            0,
            1,
            DateOnly.FromDateTime(DateTime.UtcNow),
            ModoCriacaoPlanoBiblico.Continuacao,
            ordemInicio: 11,
            planoOrigemId: Guid.NewGuid());

        var firstOrder = plan.Dias.SelectMany(x => x.Capitulos).Min(x => x.Ordem);

        Assert.Equal(11, plan.OrdemInicio);
        Assert.Equal(11, firstOrder);
        Assert.DoesNotContain(plan.Dias.SelectMany(x => x.Capitulos), x => x.Ordem <= 10);
    }

    [Fact]
    public async Task GenerateAsync_restart_starts_from_first_order()
    {
        await using var db = await CreateDbWithRealBaseAsync();
        var generator = CreateGenerator(db);

        var plan = await generator.GenerateAsync(
            Guid.NewGuid(),
            "Recomecar",
            0,
            1,
            DateOnly.FromDateTime(DateTime.UtcNow),
            ModoCriacaoPlanoBiblico.Reinicio,
            ordemInicio: 1,
            planoOrigemId: Guid.NewGuid());

        Assert.Equal(1, plan.OrdemInicio);
        Assert.Equal(1, plan.Dias.SelectMany(x => x.Capitulos).Min(x => x.Ordem));
    }

    [Fact]
    public async Task GenerateAsync_returns_clear_error_when_base_is_empty()
    {
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        var generator = CreateGenerator(db);

        var exception = await Assert.ThrowsAsync<AppException>(() => generator.GenerateAsync(
            Guid.NewGuid(),
            "Sem base",
            0,
            1,
            DateOnly.FromDateTime(DateTime.UtcNow),
            ModoCriacaoPlanoBiblico.NovoDoInicio,
            ordemInicio: 1,
            planoOrigemId: null));

        Assert.Equal(422, exception.StatusCode);
        Assert.Contains("Base biblica pastoral", exception.Message);
    }

    public void Dispose()
    {
        TryDelete(_databasePath);
        TryDelete(_databasePath + "-shm");
        TryDelete(_databasePath + "-wal");
    }

    private async Task<AppDbContext> CreateDbWithRealBaseAsync()
    {
        var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        var importer = new BaseBiblicaV2Importer(db);
        await importer.ImportAsync(FindRepositoryFile("docs/examples/base-biblica-capitulos-com-versiculos.v2.json"));
        return db;
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
        return new AppDbContext(options);
    }

    private static BiblePlanGeneratorService CreateGenerator(AppDbContext db) =>
        new(new PlanoBiblicoRepository(db));

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

    private static void TryDelete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
