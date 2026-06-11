namespace ServicoPalavra.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardMeResponse> GetMeAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
