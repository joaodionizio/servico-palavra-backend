namespace ServicoPalavra.Application.Categorias;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaResponse>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaAdminResponse>> ListAdminAsync(CancellationToken cancellationToken = default);
    Task<CategoriaAdminResponse> GetAdminAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoriaAdminResponse> CreateAsync(CategoriaRequest request, CancellationToken cancellationToken = default);
    Task<CategoriaAdminResponse> UpdateAsync(Guid id, CategoriaRequest request, CancellationToken cancellationToken = default);
    Task<CategoriaAdminResponse> UpdateStatusAsync(Guid id, CategoriaStatusRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
