using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IFavoritoRepository _favoritos;
    private readonly IPlanoBiblicoRepository _planos;
    private readonly IProgressoRepository _progressos;

    public DashboardService(IFavoritoRepository favoritos, IPlanoBiblicoRepository planos, IProgressoRepository progressos)
    {
        _favoritos = favoritos;
        _planos = planos;
        _progressos = progressos;
    }

    public async Task<DashboardMeResponse> GetMeAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var favoritos = await _favoritos.ListByUsuarioAsync(usuarioId, cancellationToken);
        var planoAtivo = await _planos.GetAtivoAsync(usuarioId, cancellationToken);

        // TODO: trocar por consulta agregada quando houver tela de dashboard mais rica.
        var concluidos = 0;
        foreach (var favorito in favoritos)
        {
            var progresso = await _progressos.GetConteudoAsync(usuarioId, favorito.ConteudoId, cancellationToken);
            if (progresso?.Status == StatusProgressoConteudo.Concluido)
            {
                concluidos++;
            }
        }

        return new DashboardMeResponse(concluidos, favoritos.Count, planoAtivo is not null);
    }
}
