using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.Abstractions;

public interface IConteudoRepository
{
    Task<IReadOnlyList<Conteudo>> ListPublicadosAsync(CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Conteudo> Items, int Total)> ListPublicadosAsync(string? busca, string? categoriaSlug, TipoConteudo? tipo, int skip, int take, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Conteudo> Items, int Total)> ListAdminAsync(string? busca, string? categoriaSlug, TipoConteudo? tipo, bool? publicado, int skip, int take, CancellationToken cancellationToken = default);
    Task<Conteudo?> GetByIdAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default);
    Task<Conteudo?> GetByIdWithMaterialsAsync(Guid id, bool includePrivate = false, bool tracking = false, CancellationToken cancellationToken = default);
    Task<Conteudo?> GetBySlugAsync(string slug, bool includePrivate = false, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default);
    Task DeleteMateriaisApoioAsync(Guid conteudoId, CancellationToken cancellationToken = default);
    void Add(Conteudo conteudo);
    void AddMaterialApoio(MaterialApoio material);
    void Remove(Conteudo conteudo);
}
