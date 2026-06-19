using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Abstractions;

public interface ICategoriaRepository
{
    Task<IReadOnlyList<CategoriaConteudo>> ListAtivasAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoriaConteudo>> ListAdminAsync(CancellationToken cancellationToken = default);
    Task<CategoriaConteudo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default);
    Task<bool> HasConteudosAsync(Guid id, CancellationToken cancellationToken = default);
    void Add(CategoriaConteudo categoria);
    void Remove(CategoriaConteudo categoria);
}
