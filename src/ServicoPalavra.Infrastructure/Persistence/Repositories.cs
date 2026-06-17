using Microsoft.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _db;
    public UsuarioRepository(AppDbContext db) => _db = db;
    public Task<ApplicationUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => _db.Users.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
}

public sealed class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _db;
    public CategoriaRepository(AppDbContext db) => _db = db;
    public async Task<IReadOnlyList<CategoriaConteudo>> ListAtivasAsync(CancellationToken cancellationToken = default) => await _db.CategoriasConteudo.Where(x => x.Ativo).OrderBy(x => x.Ordem).ThenBy(x => x.Nome).ToListAsync(cancellationToken);
    public Task<CategoriaConteudo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => _db.CategoriasConteudo.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId = null, CancellationToken cancellationToken = default) => _db.CategoriasConteudo.AnyAsync(x => x.Slug == slug && (!exceptId.HasValue || x.Id != exceptId.Value), cancellationToken);
    public void Add(CategoriaConteudo categoria) => _db.CategoriasConteudo.Add(categoria);
}

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

public sealed class FavoritoRepository : IFavoritoRepository
{
    private readonly AppDbContext _db;
    public FavoritoRepository(AppDbContext db) => _db = db;
    public async Task<IReadOnlyList<Favorito>> ListByUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default) => await _db.Favoritos.Include(x => x.Conteudo).ThenInclude(x => x.CategoriaConteudo).Where(x => x.UsuarioId == usuarioId).ToListAsync(cancellationToken);
    public Task<Favorito?> GetAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default) => _db.Favoritos.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.ConteudoId == conteudoId, cancellationToken);
    public void Add(Favorito favorito) => _db.Favoritos.Add(favorito);
    public void Remove(Favorito favorito) => _db.Favoritos.Remove(favorito);
}

public sealed class ProgressoRepository : IProgressoRepository
{
    private readonly AppDbContext _db;
    public ProgressoRepository(AppDbContext db) => _db = db;
    public Task<ProgressoConteudo?> GetConteudoAsync(Guid usuarioId, Guid conteudoId, CancellationToken cancellationToken = default) => _db.ProgressosConteudo.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.ConteudoId == conteudoId, cancellationToken);
    public void Add(ProgressoConteudo progresso) => _db.ProgressosConteudo.Add(progresso);
}

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

public sealed class PlanoBiblicoRepository : IPlanoBiblicoRepository
{
    private readonly AppDbContext _db;
    public PlanoBiblicoRepository(AppDbContext db) => _db = db;
    public Task<PlanoBiblicoUsuario?> GetAtivoAsync(Guid usuarioId, CancellationToken cancellationToken = default) => _db.PlanosBiblicosUsuario.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.Ativo && x.Status == Domain.Enums.StatusPlanoBiblico.Ativo, cancellationToken);
    public Task<PlanoBiblicoUsuario?> GetByIdAsync(Guid id, Guid usuarioId, CancellationToken cancellationToken = default) => _db.PlanosBiblicosUsuario.FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == usuarioId, cancellationToken);
    public async Task<IReadOnlyList<PlanoBiblicoDia>> ListDiasAsync(Guid planoId, Guid usuarioId, CancellationToken cancellationToken = default) => await _db.PlanosBiblicosDias.Include(x => x.PlanoBiblicoUsuario).Where(x => x.PlanoBiblicoUsuarioId == planoId && x.PlanoBiblicoUsuario.UsuarioId == usuarioId).OrderBy(x => x.DiaNumero).ToListAsync(cancellationToken);
    public Task<PlanoBiblicoDia?> GetDiaAsync(Guid diaId, Guid usuarioId, CancellationToken cancellationToken = default) => _db.PlanosBiblicosDias.Include(x => x.Capitulos).Include(x => x.PlanoBiblicoUsuario).FirstOrDefaultAsync(x => x.Id == diaId && x.PlanoBiblicoUsuario.UsuarioId == usuarioId, cancellationToken);
    public Task<PosicaoBiblicaUsuario?> GetPosicaoAsync(Guid usuarioId, CancellationToken cancellationToken = default) => _db.PosicoesBiblicasUsuario.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId, cancellationToken);
    public async Task<IReadOnlyList<BaseBiblica>> ListBaseAsync(int ordemInicio, CancellationToken cancellationToken = default) => await _db.BaseBiblica.Where(x => x.Ativo && x.Ordem >= ordemInicio).OrderBy(x => x.Ordem).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<BaseBiblica>> ListBaseAtivaAsync(CancellationToken cancellationToken = default) => await _db.BaseBiblica.Where(x => x.Ativo).OrderBy(x => x.Ordem).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<PlanoBiblicoUsuario>> ListHistoricoAsync(Guid usuarioId, CancellationToken cancellationToken = default) => await _db.PlanosBiblicosUsuario.Where(x => x.UsuarioId == usuarioId).OrderByDescending(x => x.CriadoEm).ToListAsync(cancellationToken);
    public Task<ProgressoLeitura?> GetProgressoLeituraAsync(Guid usuarioId, Guid diaId, CancellationToken cancellationToken = default) => _db.ProgressosLeitura.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.PlanoBiblicoDiaId == diaId, cancellationToken);
    public void Add(PlanoBiblicoUsuario plano) => _db.PlanosBiblicosUsuario.Add(plano);
    public void Add(ProgressoLeitura progresso) => _db.ProgressosLeitura.Add(progresso);
    public void Add(PosicaoBiblicaUsuario posicao) => _db.PosicoesBiblicasUsuario.Add(posicao);
}
