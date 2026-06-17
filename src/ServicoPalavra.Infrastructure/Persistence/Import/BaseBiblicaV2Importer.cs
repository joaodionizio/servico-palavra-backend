using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence.Import;

public sealed class BaseBiblicaV2Importer
{
    public const int ExpectedChapterCount = 1334;

    private readonly AppDbContext _db;

    public BaseBiblicaV2Importer(AppDbContext db)
    {
        _db = db;
    }

    public async Task<BaseBiblicaV2ImportResult> ImportAsync(string jsonPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jsonPath))
        {
            throw new ArgumentException("O caminho do JSON da BaseBiblica V2 deve ser informado.", nameof(jsonPath));
        }

        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException("Arquivo da BaseBiblica V2 nao encontrado.", jsonPath);
        }

        var records = await LoadAndValidateAsync(jsonPath, cancellationToken);
        var now = DateTime.UtcNow;

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        var existing = await _db.BaseBiblica.ToListAsync(cancellationToken);
        var byOrder = existing.ToDictionary(x => x.Ordem);
        var byBookChapter = existing
            .GroupBy(x => BookChapterKey(x.Livro, x.Capitulo))
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.Ordem).First());

        var inserted = 0;
        var updated = 0;

        foreach (var record in records)
        {
            var entity = byOrder.GetValueOrDefault(record.Ordem)
                ?? byBookChapter.GetValueOrDefault(BookChapterKey(record.Livro, record.Capitulo));

            if (entity is null)
            {
                entity = new BaseBiblica
                {
                    Id = Guid.NewGuid(),
                    CriadoEm = now
                };
                _db.BaseBiblica.Add(entity);
                inserted++;
            }
            else
            {
                entity.AtualizadoEm = now;
                updated++;
            }

            Apply(record, entity);
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new BaseBiblicaV2ImportResult(records.Count, inserted, updated);
    }

    private static async Task<IReadOnlyList<BaseBiblicaV2Record>> LoadAndValidateAsync(string jsonPath, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(jsonPath);
        var records = await JsonSerializer.DeserializeAsync<List<BaseBiblicaV2Record>>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("JSON da BaseBiblica V2 vazio ou invalido.");

        var failures = new List<string>();
        if (records.Count != ExpectedChapterCount)
        {
            failures.Add($"Esperados {ExpectedChapterCount} registros, encontrados {records.Count}.");
        }

        var duplicateOrders = records.GroupBy(x => x.Ordem).Where(x => x.Count() > 1).Select(x => x.Key).ToArray();
        if (duplicateOrders.Length > 0)
        {
            failures.Add($"Ordens duplicadas: {string.Join(", ", duplicateOrders.Take(20))}.");
        }

        var expectedOrders = Enumerable.Range(1, records.Count).ToArray();
        var actualOrders = records.Select(x => x.Ordem).Order().ToArray();
        if (!actualOrders.SequenceEqual(expectedOrders))
        {
            failures.Add("Ordem deve ser sequencial, sem buracos, iniciando em 1.");
        }

        var duplicateBookChapters = records
            .GroupBy(x => BookChapterKey(x.Livro, x.Capitulo))
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();
        if (duplicateBookChapters.Length > 0)
        {
            failures.Add($"Livro/capitulo duplicado: {string.Join(", ", duplicateBookChapters.Take(20))}.");
        }

        foreach (var record in records)
        {
            if (string.IsNullOrWhiteSpace(record.Livro))
            {
                failures.Add($"Ordem {record.Ordem}: Livro obrigatorio.");
            }

            if (record.Capitulo <= 0)
            {
                failures.Add($"{record.Livro}: Capitulo deve ser maior que zero.");
            }

            if (string.IsNullOrWhiteSpace(record.Grupo))
            {
                failures.Add($"{record.Livro} {record.Capitulo}: Grupo obrigatorio.");
            }

            if (string.IsNullOrWhiteSpace(record.Testamento))
            {
                failures.Add($"{record.Livro} {record.Capitulo}: Testamento obrigatorio.");
            }

            if (record.QuantidadeVersiculos <= 0)
            {
                failures.Add($"{record.Livro} {record.Capitulo}: QuantidadeVersiculos deve ser maior que zero.");
            }

            if (record.PesoLeitura <= 0)
            {
                failures.Add($"{record.Livro} {record.Capitulo}: PesoLeitura deve ser maior que zero.");
            }

            if (record.TempoEstimadoMinutos <= 0)
            {
                failures.Add($"{record.Livro} {record.Capitulo}: TempoEstimadoMinutos deve ser maior que zero.");
            }
        }

        if (failures.Count > 0)
        {
            throw new InvalidOperationException($"BaseBiblica V2 invalida: {string.Join(" ", failures.Take(30))}");
        }

        return records.OrderBy(x => x.Ordem).ToArray();
    }

    private static void Apply(BaseBiblicaV2Record record, BaseBiblica entity)
    {
        entity.Ordem = record.Ordem;
        entity.Livro = record.Livro.Trim();
        entity.Capitulo = record.Capitulo;
        entity.Grupo = record.Grupo.Trim();
        entity.Subgrupo = string.IsNullOrWhiteSpace(record.Subgrupo) ? null : record.Subgrupo.Trim();
        entity.Testamento = record.Testamento.Trim();
        entity.TempoEstimadoMinutos = record.TempoEstimadoMinutos;
        entity.QuantidadeVersiculos = record.QuantidadeVersiculos;
        entity.PesoLeitura = record.PesoLeitura;
        entity.Ativo = record.Ativo;
    }

    private static string BookChapterKey(string book, int chapter) => $"{book.Trim().ToUpperInvariant()}:{chapter}";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private sealed record BaseBiblicaV2Record(
        int Ordem,
        string Livro,
        int Capitulo,
        string Grupo,
        string? Subgrupo,
        string Testamento,
        int TempoEstimadoMinutos,
        bool Ativo,
        int QuantidadeVersiculos,
        int PesoLeitura);
}

public sealed record BaseBiblicaV2ImportResult(int Processed, int Inserted, int Updated);
