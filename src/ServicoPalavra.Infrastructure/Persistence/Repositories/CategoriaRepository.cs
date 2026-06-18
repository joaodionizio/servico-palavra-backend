using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _db;
    public CategoriaRepository(AppDbContext db) => _db = db;
    public async Task<IReadOnlyList<CategoriaConteudo>> ListAtivasAsync(CancellationToken cancellationToken = default) => await _db.CategoriasConteudo.Where(x => x.Ativo).OrderBy(x => x.Ordem).ThenBy(x => x.Nome).ToListAsync(cancellationToken);
    public Task<CategoriaConteudo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _db.CategoriasConteudo.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default) => _db.CategoriasConteudo.AnyAsync(x => x.Slug == slug && (!exceptId.HasValue || x.Id != exceptId.Value), cancellationToken);
    public void Add(CategoriaConteudo categoria) => _db.CategoriasConteudo.Add(categoria);
}
