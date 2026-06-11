namespace ServicoPalavra.Application.PlanosBiblicos;

public interface IPlanoBiblicoService
{
    Task<PlanoBiblicoResponse?> GetAtivoAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<PlanoBiblicoResponse> CriarAsync(Guid usuarioId, CriarPlanoBiblicoRequest request, CancellationToken cancellationToken = default);
    Task<PlanoBiblicoResponse> AlterarAsync(Guid usuarioId, AlterarPlanoBiblicoRequest request, CancellationToken cancellationToken = default);
    Task<PlanoBiblicoResponse> GetAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlanoBiblicoResponse>> GetHistoricoAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlanoBiblicoDiaResponse>> ListDiasAsync(Guid usuarioId, Guid planoId, CancellationToken cancellationToken = default);
    Task<PosicaoBiblicaResponse> GetPosicaoAtualAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task ConcluirDiaAsync(Guid usuarioId, Guid diaId, CancellationToken cancellationToken = default);
}
