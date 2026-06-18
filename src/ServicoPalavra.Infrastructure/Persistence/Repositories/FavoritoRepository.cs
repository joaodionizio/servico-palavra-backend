using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class FavoritoRepository : IFavoritoRepository
{
    private readonly AppDbContext _db;
    public FavoritoRepository(AppDbContext db) => _db = db;
    public async Task<IReadOnlyList<Favorito>> ListByUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default) => await _db.Favoritos.Include(x => x.Conteudo).ThenInclude(x => x.CategoriaConteudo).Where(x => x.UsuarioId == usuarioId).ToListAsync(cancellationToken);
    public Task<Favorito?> GetAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default) => _db.Favoritos.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.ConteudoId == conteudoId, cancellationToken);
    public void Add(Favorito favorito) => _db.Favoritos.Add(favorito);
    public void Remove(Favorito favorito) => _db.Favoritos.Remove(favorito);
}
