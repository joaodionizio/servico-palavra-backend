using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence.Configurations;

public sealed class BaseBiblicaConfiguration : IEntityTypeConfiguration<BaseBiblica>
{
    public void Configure(EntityTypeBuilder<BaseBiblica> builder)
    {
        builder.ToTable("BaseBiblica");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Livro).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Grupo).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Subgrupo).HasMaxLength(80);
        builder.Property(x => x.Testamento).HasMaxLength(40).IsRequired();
        builder.HasIndex(x => x.Ordem).IsUnique();
    }
}

public sealed class PlanoBiblicoUsuarioConfiguration : IEntityTypeConfiguration<PlanoBiblicoUsuario>
{
    public void Configure(EntityTypeBuilder<PlanoBiblicoUsuario> builder)
    {
        builder.ToTable("PlanosBiblicosUsuario");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(160).IsRequired();
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PlanoOrigem).WithMany().HasForeignKey(x => x.PlanoOrigemId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PlanoBiblicoDiaConfiguration : IEntityTypeConfiguration<PlanoBiblicoDia>
{
    public void Configure(EntityTypeBuilder<PlanoBiblicoDia> builder)
    {
        builder.ToTable("PlanosBiblicosDias");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LeiturasTexto).HasMaxLength(800);
        builder.HasOne(x => x.PlanoBiblicoUsuario).WithMany(x => x.Dias).HasForeignKey(x => x.PlanoBiblicoUsuarioId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PlanoBiblicoDiaCapituloConfiguration : IEntityTypeConfiguration<PlanoBiblicoDiaCapitulo>
{
    public void Configure(EntityTypeBuilder<PlanoBiblicoDiaCapitulo> builder)
    {
        builder.ToTable("PlanosBiblicosDiasCapitulos");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.PlanoBiblicoDia).WithMany(x => x.Capitulos).HasForeignKey(x => x.PlanoBiblicoDiaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.BaseBiblica).WithMany().HasForeignKey(x => x.BaseBiblicaId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ProgressoLeituraConfiguration : IEntityTypeConfiguration<ProgressoLeitura>
{
    public void Configure(EntityTypeBuilder<ProgressoLeitura> builder)
    {
        builder.ToTable("ProgressosLeitura");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UsuarioId, x.PlanoBiblicoDiaId }).IsUnique();
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.PlanoBiblicoDia).WithMany().HasForeignKey(x => x.PlanoBiblicoDiaId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class PosicaoBiblicaUsuarioConfiguration : IEntityTypeConfiguration<PosicaoBiblicaUsuario>
{
    public void Configure(EntityTypeBuilder<PosicaoBiblicaUsuario> builder)
    {
        builder.ToTable("PosicoesBiblicasUsuario");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.UsuarioId).IsUnique();
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.UltimaBaseBiblicaConcluida).WithMany().HasForeignKey(x => x.UltimaBaseBiblicaConcluidaId).OnDelete(DeleteBehavior.Restrict);
    }
}
