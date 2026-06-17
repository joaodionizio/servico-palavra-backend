using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Abstractions;

public interface IPlanoBiblicoRepository
{
    Task<PlanoBiblicoUsuario?> GetAtivoAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<PlanoBiblicoUsuario?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlanoBiblicoDia>> ListDiasAsync(Guid planoId, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<PlanoBiblicoDia?> GetDiaAsync(Guid diaId, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<PosicaoBiblicaUsuario?> GetPosicaoAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BaseBiblica>> ListBaseAsync(int ordemInicio, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BaseBiblica>> ListBaseAtivaAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlanoBiblicoUsuario>> ListHistoricoAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<ProgressoLeitura?> GetProgressoLeituraAsync(Guid usuarioId, Guid diaId, CancellationToken cancellationToken = default);
    void Add(PlanoBiblicoUsuario plano);
    void Add(ProgressoLeitura progresso);
    void Add(PosicaoBiblicaUsuario posicao);
}
