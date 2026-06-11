using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Application.Abstractions;

public interface IFavoritoRepository
{
    Task<IReadOnlyList<Favorito>> ListByUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<Favorito?> GetAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default);
    void Add(Favorito favorito);
    void Remove(Favorito favorito);
}
