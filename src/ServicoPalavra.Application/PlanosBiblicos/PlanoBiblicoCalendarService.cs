using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.PlanosBiblicos;

public sealed class PlanoBiblicoCalendarService : IPlanoBiblicoCalendarService
{
    private readonly IPlanoBiblicoRepository _planos;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICalendarClock _clock;

    public PlanoBiblicoCalendarService(
        IPlanoBiblicoRepository planos,
        IUnitOfWork unitOfWork,
        ICalendarClock clock)
    {
        _planos = planos;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public Task<bool> RealignForReadAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken = default) =>
        RealignAsync(usuarioId, planoId, _clock.Today, onlyWhenStale: true, cancellationToken);

    public Task<bool> RealignAfterCompletionAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken = default) =>
        RealignAsync(usuarioId, planoId, _clock.Today.AddDays(1), onlyWhenStale: false, cancellationToken);

    public Task<bool> RealignAfterReopenAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken = default) =>
        RealignAsync(usuarioId, planoId, _clock.Today, onlyWhenStale: false, cancellationToken);

    private async Task<bool> RealignAsync(
        Guid usuarioId,
        Guid planoId,
        DateOnly startDate,
        bool onlyWhenStale,
        CancellationToken cancellationToken)
    {
        var plano = await _planos.GetByIdForUpdateAsync(planoId, usuarioId, cancellationToken);
        if (plano is null || !plano.Ativo || plano.Status != StatusPlanoBiblico.Ativo)
        {
            return false;
        }

        var pendentes = await _planos.ListDiasPendentesForUpdateAsync(planoId, usuarioId, cancellationToken);
        var datasConcluidas = await _planos.ListDatasDiasConcluidosAsync(planoId, usuarioId, cancellationToken);
        var datasConcluidasSet = datasConcluidas.ToHashSet();
        var hoje = _clock.Today;
        var now = _clock.UtcNow.UtcDateTime;

        if (onlyWhenStale)
        {
            var datasPendentes = pendentes
                .Where(x => x.DataPrevista.HasValue)
                .Select(x => x.DataPrevista!.Value)
                .ToArray();

            var needsRealignment =
                pendentes.Any(x => !x.DataPrevista.HasValue || x.DataPrevista.Value < hoje) ||
                datasPendentes.Distinct().Count() != datasPendentes.Length ||
                datasPendentes.Any(datasConcluidasSet.Contains);

            if (!needsRealignment)
            {
                var datasAtuais = datasConcluidas.Concat(datasPendentes).ToArray();
                var expectedEnd = datasAtuais.Length > 0 ? datasAtuais.Max() : plano.DataInicio;

                if (plano.DataFimPrevista == expectedEnd)
                {
                    return false;
                }

                plano.DataFimPrevista = expectedEnd;
                plano.AtualizadoEm = now;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
        }

        var datasOcupadas = datasConcluidasSet;
        var changed = false;
        var nextDate = startDate;

        foreach (var dia in pendentes)
        {
            while (datasOcupadas.Contains(nextDate))
            {
                nextDate = nextDate.AddDays(1);
            }

            if (dia.DataPrevista != nextDate)
            {
                dia.DataPrevista = nextDate;
                dia.AtualizadoEm = now;
                changed = true;
            }

            datasOcupadas.Add(nextDate);
            nextDate = nextDate.AddDays(1);
        }

        var dataFim = datasOcupadas.Count > 0 ? datasOcupadas.Max() : plano.DataInicio;
        if (plano.DataFimPrevista != dataFim)
        {
            plano.DataFimPrevista = dataFim;
            plano.AtualizadoEm = now;
            changed = true;
        }

        if (changed)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return changed;
    }
}
