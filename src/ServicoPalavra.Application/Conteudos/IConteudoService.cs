namespace ServicoPalavra.Application.Conteudos;

public interface IConteudoService
{
    Task<PagedResponse<ConteudoResumoResponse>> ListAsync(ConteudoListQuery query, CancellationToken cancellationToken = default);
    Task<PagedResponse<ConteudoAdminResumoResponse>> ListAdminAsync(ConteudoAdminListQuery query, CancellationToken cancellationToken = default);
    Task<ConteudoResponse> GetAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default);
    Task<ConteudoAdminDetalheResponse> GetAdminAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConteudoDetalheResponse> GetBySlugAsync(string slug, Guid? usuarioId = null, CancellationToken cancellationToken = default);
    Task<ConteudoResponse> CreateAsync(ConteudoRequest request, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<ConteudoResponse> UpdateAsync(Guid id, ConteudoRequest request, CancellationToken cancellationToken = default);
    Task<ConteudoResponse> UpdatePublicacaoAsync(Guid id, ConteudoPublicacaoRequest request, CancellationToken cancellationToken = default);
    Task<ConteudoResponse> PublicarAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConteudoResponse> DespublicarAsync(Guid id, CancellationToken cancellationToken = default);
}
