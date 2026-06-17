using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Infrastructure.Persistence;
using ServicoPalavra.Infrastructure.Persistence.Import;

namespace ServicoPalavra.IntegrationTests;

public sealed class BaseBiblicaV2ImporterTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"servico-palavra-base-import-{Guid.NewGuid():N}.db");

    [Fact]
    public async Task ImportAsync_inserts_and_then_updates_without_duplicates()
    {
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        var importer = new BaseBiblicaV2Importer(db);
        var jsonPath = FindRepositoryFile("docs/examples/base-biblica-capitulos-com-versiculos.v2.json");

        var first = await importer.ImportAsync(jsonPath);
        var second = await importer.ImportAsync(jsonPath);

        Assert.Equal(1334, first.Processed);
        Assert.Equal(1334, first.Inserted);
        Assert.Equal(0, first.Updated);
        Assert.Equal(1334, second.Processed);
        Assert.Equal(0, second.Inserted);
        Assert.Equal(1334, second.Updated);
        Assert.Equal(1334, await db.BaseBiblica.CountAsync());
    }

    [Fact]
    public async Task ImportAsync_updates_existing_record_by_order()
    {
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        var existingId = Guid.NewGuid();
        db.BaseBiblica.Add(new BaseBiblica
        {
            Id = existingId,
            Ordem = 1,
            Livro = "Registro antigo",
            Capitulo = 99,
            Grupo = "Grupo antigo",
            Testamento = "Antigo",
            CriadoEm = DateTime.UtcNow.AddDays(-1),
            QuantidadeVersiculos = 1,
            PesoLeitura = 1
        });
        await db.SaveChangesAsync();

        var importer = new BaseBiblicaV2Importer(db);
        var result = await importer.ImportAsync(FindRepositoryFile("docs/examples/base-biblica-capitulos-com-versiculos.v2.json"));
        var updated = await db.BaseBiblica.SingleAsync(x => x.Id == existingId);

        Assert.Equal(1333, result.Inserted);
        Assert.Equal(1, result.Updated);
        Assert.Equal("Gênesis", updated.Livro);
        Assert.Equal(1, updated.Capitulo);
        Assert.Equal(31, updated.QuantidadeVersiculos);
        Assert.Equal(31, updated.PesoLeitura);
        Assert.NotNull(updated.AtualizadoEm);
        Assert.Equal(1334, await db.BaseBiblica.CountAsync());
    }

    [Fact]
    public async Task ImportAsync_rejects_invalid_json_before_writing()
    {
        await using var db = CreateDbContext();
        await db.Database.EnsureCreatedAsync();
        var invalidPath = Path.Combine(Path.GetTempPath(), $"base-biblica-invalid-{Guid.NewGuid():N}.json");
        var records = JsonSerializer.Deserialize<List<JsonElement>>(await File.ReadAllTextAsync(FindRepositoryFile("docs/examples/base-biblica-capitulos-com-versiculos.v2.json")))!;
        var first = records[0].Deserialize<Dictionary<string, object?>>()!;
        first["quantidadeVersiculos"] = 0;
        records[0] = JsonSerializer.SerializeToElement(first);
        await File.WriteAllTextAsync(invalidPath, JsonSerializer.Serialize(records));

        try
        {
            var importer = new BaseBiblicaV2Importer(db);

            await Assert.ThrowsAsync<InvalidOperationException>(() => importer.ImportAsync(invalidPath));
            Assert.Equal(0, await db.BaseBiblica.CountAsync());
        }
        finally
        {
            if (File.Exists(invalidPath))
            {
                File.Delete(invalidPath);
            }
        }
    }

    public void Dispose()
    {
        TryDelete(_databasePath);
        TryDelete(_databasePath + "-shm");
        TryDelete(_databasePath + "-wal");
    }

    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
        return new AppDbContext(options);
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

    private static void TryDelete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
