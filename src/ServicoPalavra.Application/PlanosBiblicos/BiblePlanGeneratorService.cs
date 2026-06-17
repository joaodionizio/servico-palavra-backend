using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Common;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.PlanosBiblicos;

public sealed class BiblePlanGeneratorService : IBiblePlanGeneratorService
{
    private const int DaysPerYear = 365;
    private const int DaysPerMonth = 30;
    private const int MaxDurationDays = DaysPerYear * 10;
    private const string PsalmsBookName = "Salmos";

    private readonly IPlanoBiblicoRepository _planos;

    public BiblePlanGeneratorService(IPlanoBiblicoRepository planos) => _planos = planos;

    public async Task<PlanoBiblicoUsuario> GenerateAsync(
        Guid usuarioId,
        string nome,
        int duracaoAnos,
        int duracaoMeses,
        DateOnly dataInicio,
        ModoCriacaoPlanoBiblico modo,
        int ordemInicio,
        Guid? planoOrigemId,
        CancellationToken cancellationToken = default)
    {
        var duracaoDias = CalculateDurationDays(duracaoAnos, duracaoMeses);
        var baseBiblica = await _planos.ListBaseAtivaAsync(cancellationToken);
        if (baseBiblica.Count == 0)
        {
            throw new AppException("Base biblica pastoral nao cadastrada para a ordem solicitada.", 422);
        }

        var sequence = BuildPastoralSequence(baseBiblica)
            .Where(x => x.Ordem >= ordemInicio)
            .ToList();
        if (sequence.Count == 0)
        {
            throw new AppException("Sequencia pastoral nao possui leituras para a ordem solicitada.", 422);
        }

        var psalms = baseBiblica
            .Where(x => x.Livro == PsalmsBookName)
            .OrderBy(x => x.Capitulo)
            .ToList();

        var plano = new PlanoBiblicoUsuario
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            Nome = string.IsNullOrWhiteSpace(nome) ? "Plano biblico" : nome.Trim(),
            DuracaoDias = duracaoDias,
            DuracaoAnos = duracaoAnos,
            DuracaoMeses = duracaoMeses,
            DataInicio = dataInicio,
            Ativo = true,
            Status = StatusPlanoBiblico.Ativo,
            PlanoOrigemId = planoOrigemId,
            ModoCriacao = modo,
            OrdemInicio = sequence[0].Ordem,
            OrdemFim = sequence[^1].Ordem,
            CriadoEm = DateTime.UtcNow
        };

        var dayCount = duracaoDias;
        var groups = Distribute(sequence, psalms, dayCount);
        for (var i = 0; i < groups.Count; i++)
        {
            var day = groups[i];
            var chapters = day.Chapters;
            var leituras = chapters.Select(x => $"{x.DisplayBook} {x.Chapter.Capitulo}").ToList();
            if (day.Psalm is not null)
            {
                leituras.Add($"{day.Psalm.Livro} {day.Psalm.Capitulo}");
            }

            var leiturasTexto = leituras.Count == 0
                ? "Meditação livre"
                : string.Join(", ", leituras);
            var dia = new PlanoBiblicoDia
            {
                Id = Guid.NewGuid(),
                PlanoBiblicoUsuarioId = plano.Id,
                PlanoBiblicoUsuario = plano,
                DiaNumero = i + 1,
                MesNumero = (i / DaysPerMonth) + 1,
                DataPrevista = dataInicio.AddDays(i),
                LeiturasTexto = leiturasTexto,
                CriadoEm = DateTime.UtcNow
            };

            foreach (var chapter in chapters)
            {
                dia.Capitulos.Add(new PlanoBiblicoDiaCapitulo
                {
                    Id = Guid.NewGuid(),
                    PlanoBiblicoDiaId = dia.Id,
                    PlanoBiblicoDia = dia,
                    BaseBiblicaId = chapter.Chapter.Id,
                    BaseBiblica = chapter.Chapter,
                    Ordem = chapter.Ordem
                });
            }

            plano.Dias.Add(dia);
        }

        plano.DataFimPrevista = dataInicio.AddDays(Math.Max(1, plano.Dias.Count) - 1);
        plano.OrdemFim = plano.Dias.SelectMany(x => x.Capitulos).Max(x => (int?)x.Ordem);
        return plano;
    }

    private static int CalculateDurationDays(int years, int months)
    {
        if (years < 0 || months < 0)
        {
            throw new AppException("Duracao do plano deve ser positiva.", 400);
        }

        var days = (years * DaysPerYear) + (months * DaysPerMonth);
        if (days <= 0)
        {
            throw new AppException("Informe duracao minima de 1 mes.", 400);
        }

        if (days > MaxDurationDays)
        {
            throw new AppException("Duracao maxima do plano biblico e de 10 anos.", 400);
        }

        return days;
    }

    private static List<DayReadingPlan> Distribute(IReadOnlyList<PastoralChapter> chapters, IReadOnlyList<BaseBiblica> psalms, int dayCount)
    {
        if (dayCount <= 0)
        {
            return [];
        }

        var result = Enumerable.Range(0, dayCount)
            .Select(index => new DayReadingPlan(index < psalms.Count ? psalms[index] : null))
            .ToList();
        var totalWeight = chapters.Sum(x => ReadingWeight(x.Chapter)) + result.Sum(x => x.Psalm is { } psalm ? ReadingWeight(psalm) : 0);
        if (totalWeight <= 0)
        {
            throw new AppException("Base biblica pastoral sem peso de leitura valido.", 422);
        }

        var targetWeightPerDay = totalWeight / (double)dayCount;
        var chapterIndex = 0;
        var assignedWeight = 0;

        for (var dayIndex = 0; dayIndex < dayCount && chapterIndex < chapters.Count; dayIndex++)
        {
            assignedWeight += result[dayIndex].Psalm is { } psalm ? ReadingWeight(psalm) : 0;
            var remainingDaysIncludingThis = dayCount - dayIndex;
            if (chapters.Count - chapterIndex <= remainingDaysIncludingThis)
            {
                result[dayIndex].Chapters.Add(chapters[chapterIndex]);
                assignedWeight += ReadingWeight(chapters[chapterIndex].Chapter);
                chapterIndex++;
                continue;
            }

            var dayTargetCumulativeWeight = targetWeightPerDay * (dayIndex + 1);
            var remainingDaysAfterThis = dayCount - dayIndex - 1;

            while (chapterIndex < chapters.Count)
            {
                var chapter = chapters[chapterIndex];
                var chapterWeight = ReadingWeight(chapter.Chapter);

                if (result[dayIndex].Chapters.Count > 0
                    && assignedWeight + chapterWeight > dayTargetCumulativeWeight
                    && chapters.Count - chapterIndex > remainingDaysAfterThis)
                {
                    var currentGap = Math.Abs(dayTargetCumulativeWeight - assignedWeight);
                    var withChapterGap = Math.Abs(dayTargetCumulativeWeight - (assignedWeight + chapterWeight));
                    if (currentGap <= withChapterGap)
                    {
                        break;
                    }
                }

                result[dayIndex].Chapters.Add(chapter);
                assignedWeight += chapterWeight;
                chapterIndex++;

                if (assignedWeight >= dayTargetCumulativeWeight && chapters.Count - chapterIndex >= remainingDaysAfterThis)
                {
                    break;
                }
            }
        }

        if (chapterIndex < chapters.Count)
        {
            result[^1].Chapters.AddRange(chapters.Skip(chapterIndex));
        }

        return result;
    }

    private static IReadOnlyList<PastoralChapter> BuildPastoralSequence(IReadOnlyList<BaseBiblica> baseBiblica)
    {
        var byBook = baseBiblica
            .GroupBy(x => x.Livro)
            .ToDictionary(x => x.Key, x => x.OrderBy(c => c.Capitulo).ToList(), StringComparer.OrdinalIgnoreCase);
        var sequence = new List<PastoralChapter>();
        var ordem = 1;

        foreach (var segment in PastoralSegments)
        {
            if (!byBook.TryGetValue(segment.Book, out var chapters))
            {
                throw new AppException($"Livro ausente na BaseBiblica: {segment.DisplayBook}.", 422);
            }

            var start = segment.StartChapter ?? chapters[0].Capitulo;
            var end = segment.EndChapter ?? chapters[^1].Capitulo;
            var selected = chapters.Where(x => x.Capitulo >= start && x.Capitulo <= end).ToList();
            var expectedCount = end - start + 1;
            if (selected.Count != expectedCount)
            {
                throw new AppException($"Faixa invalida na sequencia pastoral: {segment.DisplayBook} {start}-{end}.", 422);
            }

            for (var repetition = 0; repetition < segment.Repetitions; repetition++)
            {
                foreach (var chapter in selected)
                {
                    sequence.Add(new PastoralChapter(chapter, ordem++, segment.DisplayBook));
                }
            }
        }

        return sequence;
    }

    private static int ReadingWeight(BaseBiblica chapter)
    {
        if (chapter.PesoLeitura > 0)
        {
            return chapter.PesoLeitura;
        }

        if (chapter.QuantidadeVersiculos > 0)
        {
            return chapter.QuantidadeVersiculos;
        }

        return chapter.TempoEstimadoMinutos.GetValueOrDefault();
    }

    private sealed record PastoralSegment(string Book, string DisplayBook, int? StartChapter = null, int? EndChapter = null, int Repetitions = 1);
    private sealed record PastoralChapter(BaseBiblica Chapter, int Ordem, string DisplayBook);
    private sealed record DayReadingPlan(BaseBiblica? Psalm)
    {
        public List<PastoralChapter> Chapters { get; } = [];
    }

    private static readonly IReadOnlyList<PastoralSegment> PastoralSegments =
    [
        new("1 João", "1 João", Repetitions: 2),
        new("João", "João"),
        new("Marcos", "Marcos"),
        new("Gálatas", "Gálatas"),
        new("Efésios", "Efésios"),
        new("Filipenses", "Filipenses"),
        new("Colossenses", "Colossenses"),
        new("1 Tessalonicenses", "1 Tessalonicenses"),
        new("2 Tessalonicenses", "2 Tessalonicenses"),
        new("1 Timóteo", "1 Timóteo"),
        new("2 Timóteo", "2 Timóteo"),
        new("Tito", "Tito"),
        new("Filemon", "Filêmon"),
        new("Lucas", "Lucas"),
        new("Atos", "Atos"),
        new("Romanos", "Romanos"),
        new("Mateus", "Mateus"),
        new("1 Coríntios", "1 Coríntios"),
        new("2 Coríntios", "2 Coríntios"),
        new("Hebreus", "Hebreus"),
        new("Tiago", "Tiago"),
        new("1 Pedro", "1 Pedro"),
        new("2 Pedro", "2 Pedro"),
        new("2 João", "2 João"),
        new("3 João", "3 João"),
        new("Judas", "Judas"),
        new("Apocalipse", "Apocalipse"),
        new("1 João", "1 João"),
        new("João", "João"),
        new("Sabedoria", "Sabedoria"),
        new("Eclesiástico", "Eclesiástico"),
        new("Provérbios", "Provérbios"),
        new("Gênesis", "Gênesis"),
        new("Êxodo", "Êxodo"),
        new("Números", "Números"),
        new("Josué", "Josué"),
        new("Juízes", "Juízes"),
        new("1 Samuel", "1 Samuel"),
        new("2 Samuel", "2 Samuel"),
        new("1 Reis", "1 Reis"),
        new("2 Reis", "2 Reis"),
        new("Amós", "Amós"),
        new("Oseias", "Oseias"),
        new("Isaías", "Isaías", 1, 39),
        new("Miqueias", "Miqueias"),
        new("Naum", "Naum"),
        new("Sofonias", "Sofonias"),
        new("Habacuc", "Habacuc"),
        new("Jeremias", "Jeremias"),
        new("Lamentações", "Lamentações"),
        new("Ezequiel", "Ezequiel"),
        new("Abdias", "Abdias"),
        new("Isaías", "Isaías", 40, 55),
        new("1 Crônicas", "1 Crônicas"),
        new("2 Crônicas", "2 Crônicas"),
        new("Esdras", "Esdras"),
        new("Neemias", "Neemias"),
        new("Ageu", "Ageu"),
        new("Zacarias", "Zacarias"),
        new("Isaías", "Isaías", 56, 66),
        new("Malaquias", "Malaquias"),
        new("Joel", "Joel"),
        new("Jonas", "Jonas"),
        new("Rute", "Rute"),
        new("Tobias", "Tobias"),
        new("Judite", "Judite"),
        new("Ester", "Ester"),
        new("Cântico dos Cânticos", "Cântico dos Cânticos"),
        new("Jó", "Jó"),
        new("Eclesiastes", "Eclesiastes"),
        new("1 Macabeus", "1 Macabeus"),
        new("2 Macabeus", "2 Macabeus"),
        new("Baruc", "Baruc"),
        new("Daniel", "Daniel"),
        new("Levítico", "Levítico"),
        new("Deuteronômio", "Deuteronômio")
    ];
}
