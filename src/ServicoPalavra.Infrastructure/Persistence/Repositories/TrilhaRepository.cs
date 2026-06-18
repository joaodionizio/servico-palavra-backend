using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class TrilhaRepository : ITrilhaRepository
{
    private readonly AppDbContext _db;
    public TrilhaRepository(AppDbContext db) => _db = db;
    public async Task<IReadOnlyList<TrilhaFormacao>> ListPublicadasAsync(CancellationToken cancellationToken = default) => await Query().Where(x => x.Publicado).OrderBy(x => x.Ordem).ThenBy(x => x.Titulo).ToListAsync(cancellationToken);
    public Task<TrilhaFormacao?> GetByIdAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default) => Query().FirstOrDefaultAsync(x => x.Id == id && (includePrivate || x.Publicado), cancellationToken);
    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default) => _db.TrilhasFormacao.AnyAsync(x => x.Slug == slug && (!exceptId.HasValue || x.Id != exceptId.Value), cancellationToken);
    public Task<TrilhaConteudo?> GetConteudoAsync(Guid trilhaId, Guid conteudoId, CancellationToken cancellationToken = default) => _db.TrilhasConteudos.FirstOrDefaultAsync(x => x.TrilhaFormacaoId == trilhaId && x.ConteudoId == conteudoId, cancellationToken);
    public void Add(TrilhaFormacao trilha) => _db.TrilhasFormacao.Add(trilha);
    public void AddConteudo(TrilhaConteudo trilhaConteudo) => _db.TrilhasConteudos.Add(trilhaConteudo);
    public void RemoveConteudo(TrilhaConteudo trilhaConteudo) => _db.TrilhasConteudos.Remove(trilhaConteudo);
    private IQueryable<TrilhaFormacao> Query() => _db.TrilhasFormacao.Include(x => x.Conteudos).ThenInclude(x => x.Conteudo).Include(x => x.CriadoPorUsuario);
}
