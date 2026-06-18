using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence.Configurations;

public sealed class CategoriaConteudoConfiguration : IEntityTypeConfiguration<CategoriaConteudo>
{
    public void Configure(EntityTypeBuilder<CategoriaConteudo> builder)
    {
        builder.ToTable("CategoriasConteudo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(140).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(600);
        builder.Property(x => x.Cor).HasMaxLength(30);
        builder.Property(x => x.Icone).HasMaxLength(80);
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}

public sealed class ConteudoConfiguration : IEntityTypeConfiguration<Conteudo>
{
    public void Configure(EntityTypeBuilder<Conteudo> builder)
    {
        builder.ToTable("Conteudos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Titulo).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(220).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(2000);
        builder.Property(x => x.Resumo).HasMaxLength(500);
        builder.Property(x => x.Url).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.UrlThumbnail).HasMaxLength(1000);
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasOne(x => x.CategoriaConteudo).WithMany(x => x.Conteudos).HasForeignKey(x => x.CategoriaConteudoId).IsRequired(false).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CriadoPorUsuario).WithMany(x => x.ConteudosCriados).HasForeignKey(x => x.CriadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class MaterialApoioConfiguration : IEntityTypeConfiguration<MaterialApoio>
{
    public void Configure(EntityTypeBuilder<MaterialApoio> builder)
    {
        builder.ToTable("MateriaisApoio");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Titulo).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(600);
        builder.Property(x => x.Url).HasMaxLength(1000).IsRequired();
        builder.HasOne(x => x.Conteudo).WithMany(x => x.MateriaisApoio).HasForeignKey(x => x.ConteudoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ProgressoConteudoConfiguration : IEntityTypeConfiguration<ProgressoConteudo>
{
    public void Configure(EntityTypeBuilder<ProgressoConteudo> builder)
    {
        builder.ToTable("ProgressosConteudo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Percentual).HasPrecision(5, 2).IsRequired();
        builder.HasIndex(x => new { x.UsuarioId, x.ConteudoId }).IsUnique();
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Conteudo).WithMany().HasForeignKey(x => x.ConteudoId).OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class FavoritoConfiguration : IEntityTypeConfiguration<Favorito>
{
    public void Configure(EntityTypeBuilder<Favorito> builder)
    {
        builder.ToTable("Favoritos");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.UsuarioId, x.ConteudoId }).IsUnique();
        builder.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Conteudo).WithMany().HasForeignKey(x => x.ConteudoId).OnDelete(DeleteBehavior.Cascade);
    }
}
