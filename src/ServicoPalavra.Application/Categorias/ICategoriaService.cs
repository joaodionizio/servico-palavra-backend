namespace ServicoPalavra.Application.Categorias;

public interface ICategoriaService
{
    Task<IReadOnlyList<CategoriaResponse>> ListAsync(CancellationToken cancellationToken = default);
    Task<CategoriaResponse> CreateAsync(CategoriaRequest request, CancellationToken cancellationToken = default);
    Task<CategoriaResponse> UpdateAsync(Guid id, CategoriaRequest request, CancellationToken cancellationToken = default);
}
