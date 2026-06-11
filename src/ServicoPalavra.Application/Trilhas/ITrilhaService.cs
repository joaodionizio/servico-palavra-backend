namespace ServicoPalavra.Application.Trilhas;

public interface ITrilhaService
{
    Task<IReadOnlyList<TrilhaResponse>> ListAsync(CancellationToken cancellationToken = default);
    Task<TrilhaResponse> GetAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default);
    Task<TrilhaResponse> CreateAsync(TrilhaRequest request, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<TrilhaResponse> UpdateAsync(Guid id, TrilhaRequest request, CancellationToken cancellationToken = default);
    Task AddConteudoAsync(Guid trilhaId, AddTrilhaConteudoRequest request, CancellationToken cancellationToken = default);
    Task ReordenarConteudosAsync(Guid trilhaId, ReordenarTrilhaConteudoRequest request, CancellationToken cancellationToken = default);
    Task RemoveConteudoAsync(Guid trilhaId, Guid conteudoId, CancellationToken cancellationToken = default);
}
