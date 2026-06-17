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
        var baseBiblica = await _planos.ListBaseAsync(ordemInicio, cancellationToken);
        if (baseBiblica.Count == 0)
        {
            throw new AppException("Base biblica pastoral nao cadastrada para a ordem solicitada.", 422);
        }

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
            OrdemInicio = baseBiblica[0].Ordem,
            OrdemFim = baseBiblica[^1].Ordem,
            CriadoEm = DateTime.UtcNow
        };

        var dayCount = duracaoDias;
        var groups = Distribute(baseBiblica, dayCount);
        for (var i = 0; i < groups.Count; i++)
        {
            var chapters = groups[i];
            var leiturasTexto = chapters.Count == 0
                ? "Meditação livre"
                : string.Join(", ", chapters.Select(x => $"{x.Livro} {x.Capitulo}"));
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
                    BaseBiblicaId = chapter.Id,
                    BaseBiblica = chapter,
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

    private static List<List<BaseBiblica>> Distribute(IReadOnlyList<BaseBiblica> chapters, int dayCount)
    {
        if (dayCount <= 0)
        {
            return [];
        }

        var result = Enumerable.Range(0, dayCount).Select(_ => new List<BaseBiblica>()).ToList();
        var totalWeight = chapters.Sum(ReadingWeight);
        if (totalWeight <= 0)
        {
            throw new AppException("Base biblica pastoral sem peso de leitura valido.", 422);
        }

        var targetWeightPerDay = totalWeight / (double)dayCount;
        var chapterIndex = 0;
        var assignedWeight = 0;

        for (var dayIndex = 0; dayIndex < dayCount && chapterIndex < chapters.Count; dayIndex++)
        {
            var remainingDaysIncludingThis = dayCount - dayIndex;
            if (chapters.Count - chapterIndex <= remainingDaysIncludingThis)
            {
                result[dayIndex].Add(chapters[chapterIndex]);
                assignedWeight += ReadingWeight(chapters[chapterIndex]);
                chapterIndex++;
                continue;
            }

            var dayTargetCumulativeWeight = targetWeightPerDay * (dayIndex + 1);
            var remainingDaysAfterThis = dayCount - dayIndex - 1;

            while (chapterIndex < chapters.Count)
            {
                var chapter = chapters[chapterIndex];
                var chapterWeight = ReadingWeight(chapter);

                if (result[dayIndex].Count > 0
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

                result[dayIndex].Add(chapter);
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
            result[^1].AddRange(chapters.Skip(chapterIndex));
        }

        return result;
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
}
