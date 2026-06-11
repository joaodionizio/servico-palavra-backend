using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Abstractions;

public interface IConteudoRepository
{
    Task<IReadOnlyList<Conteudo>> ListPublicadosAsync(CancellationToken cancellationToken = default);
    Task<Conteudo?> GetByIdAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default);
    void Add(Conteudo conteudo);
}
