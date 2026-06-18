using ServicoPalavra.Application.Abstractions;

namespace ServicoPalavra.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardReadRepository _dashboard;

    public DashboardService(IDashboardReadRepository dashboard)
    {
        _dashboard = dashboard;
    }

    public Task<DashboardMeResponse> GetMeAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        _dashboard.GetMeAsync(usuarioId, cancellationToken);
}
