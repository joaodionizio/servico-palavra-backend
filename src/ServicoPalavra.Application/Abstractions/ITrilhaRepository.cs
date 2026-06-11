using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Abstractions;

public interface ITrilhaRepository
{
    Task<IReadOnlyList<TrilhaFormacao>> ListPublicadasAsync(CancellationToken cancellationToken = default);
    Task<TrilhaFormacao?> GetByIdAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default);
    Task<TrilhaConteudo?> GetConteudoAsync(Guid trilhaId, Guid conteudoId, CancellationToken cancellationToken = default);
    void Add(TrilhaFormacao trilha);
    void AddConteudo(TrilhaConteudo trilhaConteudo);
    void RemoveConteudo(TrilhaConteudo trilhaConteudo);
}
