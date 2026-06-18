using ServicoPalavra.Application.Dashboard;

namespace ServicoPalavra.Application.Abstractions;

public interface IDashboardReadRepository
{
    Task<DashboardMeResponse> GetMeAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}
