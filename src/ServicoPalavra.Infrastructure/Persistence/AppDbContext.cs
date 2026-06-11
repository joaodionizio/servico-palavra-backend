using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Perfil> Perfis => Set<Perfil>();
    public new DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<CategoriaConteudo> CategoriasConteudo => Set<CategoriaConteudo>();
    public DbSet<Conteudo> Conteudos => Set<Conteudo>();
    public DbSet<MaterialApoio> MateriaisApoio => Set<MaterialApoio>();
    public DbSet<TrilhaFormacao> TrilhasFormacao => Set<TrilhaFormacao>();
    public DbSet<TrilhaConteudo> TrilhasConteudos => Set<TrilhaConteudo>();
    public DbSet<ProgressoConteudo> ProgressosConteudo => Set<ProgressoConteudo>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<BaseBiblica> BaseBiblica => Set<BaseBiblica>();
    public DbSet<PlanoBiblicoUsuario> PlanosBiblicosUsuario => Set<PlanoBiblicoUsuario>();
    public DbSet<PlanoBiblicoDia> PlanosBiblicosDias => Set<PlanoBiblicoDia>();
    public DbSet<PlanoBiblicoDiaCapitulo> PlanosBiblicosDiasCapitulos => Set<PlanoBiblicoDiaCapitulo>();
    public DbSet<ProgressoLeitura> ProgressosLeitura => Set<ProgressoLeitura>();
    public DbSet<PosicaoBiblicaUsuario> PosicoesBiblicasUsuario => Set<PosicaoBiblicaUsuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        var strategy = Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);
            await action();
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
