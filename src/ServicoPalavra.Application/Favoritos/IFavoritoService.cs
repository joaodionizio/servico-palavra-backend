using ServicoPalavra.Application.Conteudos;

namespace ServicoPalavra.Application.Favoritos;

public interface IFavoritoService
{
    Task<IReadOnlyList<ConteudoResponse>> ListAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task FavoritarAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default);
}
