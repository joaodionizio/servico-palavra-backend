using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Entities;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class ConteudoRepository : IConteudoRepository
{
    private readonly AppDbContext _db;
    public ConteudoRepository(AppDbContext db) => _db = db;
    public async Task<IReadOnlyList<Conteudo>> ListPublicadosAsync(CancellationToken cancellationToken = default) => await ReadQuery().Where(x => x.Publicado).OrderBy(x => x.Ordem).ThenBy(x => x.Titulo).ToListAsync(cancellationToken);
    public async Task<(IReadOnlyList<Conteudo> Items, int Total)> ListPublicadosAsync(string? busca, string? categoriaSlug, TipoConteudo? tipo, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = ReadQuery().Where(x => x.Publicado);

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var term = busca.Trim();
            query = query.Where(x => x.Titulo.Contains(term) || (x.Resumo != null && x.Resumo.Contains(term)) || (x.Descricao != null && x.Descricao.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(categoriaSlug))
        {
            var slug = categoriaSlug.Trim();
            query = query.Where(x => x.CategoriaConteudo != null && x.CategoriaConteudo.Ativo && x.CategoriaConteudo.Slug == slug);
        }

        if (tipo.HasValue)
        {
            query = query.Where(x => x.Tipo == tipo.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Ordem)
            .ThenByDescending(x => x.PublicadoEm ?? x.CriadoEm)
            .ThenBy(x => x.Titulo)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
    public async Task<(IReadOnlyList<Conteudo> Items, int Total)> ListAdminAsync(string? busca, string? categoriaSlug, TipoConteudo? tipo, bool? publicado, int skip, int take, CancellationToken cancellationToken = default)
    {
        var query = ReadQuery();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var term = busca.Trim();
            query = query.Where(x => x.Titulo.Contains(term) || (x.Resumo != null && x.Resumo.Contains(term)) || (x.Descricao != null && x.Descricao.Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(categoriaSlug))
        {
            var slug = categoriaSlug.Trim();
            query = query.Where(x => x.CategoriaConteudo != null && x.CategoriaConteudo.Slug == slug);
        }

        if (tipo.HasValue)
        {
            query = query.Where(x => x.Tipo == tipo.Value);
        }

        if (publicado.HasValue)
        {
            query = query.Where(x => x.Publicado == publicado.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(x => x.Ordem)
            .ThenByDescending(x => x.PublicadoEm ?? x.CriadoEm)
            .ThenBy(x => x.Titulo)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
    public Task<Conteudo?> GetByIdAsync(Guid id, bool includePrivate = false, CancellationToken cancellationToken = default) => Query().FirstOrDefaultAsync(x => x.Id == id && (includePrivate || x.Publicado), cancellationToken);
    public Task<Conteudo?> GetByIdWithMaterialsAsync(Guid id, bool includePrivate = false, bool tracking = false, CancellationToken cancellationToken = default)
    {
        var query = tracking ? Query() : ReadQuery();
        return query.Include(x => x.MateriaisApoio).FirstOrDefaultAsync(x => x.Id == id && (includePrivate || x.Publicado), cancellationToken);
    }
    public Task<Conteudo?> GetBySlugAsync(string slug, bool includePrivate = false, CancellationToken cancellationToken = default) => ReadQuery(includeMaterials: true).FirstOrDefaultAsync(x => x.Slug == slug && (includePrivate || x.Publicado), cancellationToken);
    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default) => _db.Conteudos.AnyAsync(x => x.Slug == slug && (!exceptId.HasValue || x.Id != exceptId.Value), cancellationToken);
    public async Task DeleteMateriaisApoioAsync(Guid conteudoId, CancellationToken cancellationToken = default) => await _db.MateriaisApoio.Where(x => x.ConteudoId == conteudoId).ExecuteDeleteAsync(cancellationToken);
    public void Add(Conteudo conteudo) => _db.Conteudos.Add(conteudo);
    public void AddMaterialApoio(MaterialApoio material) => _db.MateriaisApoio.Add(material);
    public void Remove(Conteudo conteudo) => _db.Conteudos.Remove(conteudo);
    private IQueryable<Conteudo> Query() => _db.Conteudos.Include(x => x.CategoriaConteudo).Include(x => x.CriadoPorUsuario);
    private IQueryable<Conteudo> ReadQuery(bool includeMaterials = false)
    {
        var query = _db.Conteudos.AsNoTracking().Include(x => x.CategoriaConteudo).Include(x => x.CriadoPorUsuario);
        return includeMaterials ? query.Include(x => x.MateriaisApoio) : query;
    }
}
