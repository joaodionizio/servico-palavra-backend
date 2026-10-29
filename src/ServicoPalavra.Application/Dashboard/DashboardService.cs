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
        var result = await _dashboard.GetMeAsync(usuarioId, today, cancellationToken);

        if (!result.NeedsCalendarRealignment || result.Response.PlanoBiblicoAtivo is null)
        {
            return result.Response;
        }

        var changed = await _calendar.RealignForReadAsync(
            usuarioId,
            result.Response.PlanoBiblicoAtivo.Id,
            cancellationToken);

        if (!changed)
        {
            return result.Response;
        }

        return (await _dashboard.GetMeAsync(usuarioId, today, cancellationToken)).Response;
    }
}
