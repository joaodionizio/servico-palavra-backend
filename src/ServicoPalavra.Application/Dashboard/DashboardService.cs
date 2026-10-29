using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.PlanosBiblicos;

namespace ServicoPalavra.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardReadRepository _dashboard;
    private readonly IPlanoBiblicoCalendarService _calendar;
    private readonly ICalendarClock _clock;

    public DashboardService(
        IDashboardReadRepository dashboard,
        IPlanoBiblicoCalendarService calendar,
        ICalendarClock clock)
    {
        _dashboard = dashboard;
        _calendar = calendar;
        _clock = clock;
    }

    public async Task<DashboardMeResponse> GetMeAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var today = _clock.Today;
        var response = await _dashboard.GetMeAsync(usuarioId, today, cancellationToken);
        var leitura = response.LeituraHoje;

        var needsRealignment = response.PlanoBiblicoAtivo is not null
            && leitura is not null
            && (!leitura.DataPrevista.HasValue || leitura.DataPrevista.Value < today);

        if (!needsRealignment)
        {
            return response;
        }

        var changed = await _calendar.RealignForReadAsync(
            usuarioId,
            response.PlanoBiblicoAtivo!.Id,
            cancellationToken);

        return changed
            ? await _dashboard.GetMeAsync(usuarioId, today, cancellationToken)
            : response;
    }
}
