using ServicoPalavra.Application.Dashboard;

namespace ServicoPalavra.Application.Abstractions;

public sealed record DashboardReadResult(
    DashboardMeResponse Response,
    bool NeedsCalendarRealignment);

public interface IDashboardReadRepository
{
    Task<DashboardReadResult> GetMeAsync(Guid usuarioId, DateOnly today, CancellationToken cancellationToken = default);
}
