using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class ConteudoRepository : IConteudoRepository
{
    private readonly AppDbContext _db;
    public ConteudoRepository(AppDbContext db) => _db = db;
    public async Task<IReadOnlyList<Conteudo>> ListPublicadosAsync(CancellationToken cancellationToken = default) => await Query().Where(x => x.Publicado).OrderBy(x => x.Ordem).ThenBy(x => x.Titulo).ToListAsync(cancellationToken);
    public Task<Conteudo?> GetByIdAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default) => Query().FirstOrDefaultAsync(x => x.Id == id && (includePrivate || x.Publicado), cancellationToken);
    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default) => _db.Conteudos.AnyAsync(x => x.Slug == slug && (!exceptId.HasValue || x.Id != exceptId.Value), cancellationToken);
    public void Add(Conteudo conteudo) => _db.Conteudos.Add(conteudo);
    private IQueryable<Conteudo> Query() => _db.Conteudos.Include(x => x.CategoriaConteudo).Include(x => x.CriadoPorUsuario);
}
