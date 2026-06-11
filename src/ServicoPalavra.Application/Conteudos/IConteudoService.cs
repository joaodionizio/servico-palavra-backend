namespace ServicoPalavra.Application.Conteudos;

public interface IConteudoService
{
    Task<IReadOnlyList<ConteudoResponse>> ListAsync(CancellationToken cancellationToken = default);
    Task<ConteudoResponse> GetAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default);
    Task<ConteudoResponse> CreateAsync(ConteudoRequest request, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<ConteudoResponse> UpdateAsync(Guid id, ConteudoRequest request, CancellationToken cancellationToken = default);
    Task<ConteudoResponse> PublicarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConteudoResponse> DespublicarAsync(Guid id, CancellationToken cancellationToken = default);
}
