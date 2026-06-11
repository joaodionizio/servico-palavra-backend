using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence.Configurations;

public sealed class TrilhaFormacaoConfiguration : IEntityTypeConfiguration<TrilhaFormacao>
{
    public void Configure(EntityTypeBuilder<TrilhaFormacao> builder)
    {
        builder.ToTable("TrilhasFormacao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Titulo).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(220).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(2000);
        builder.Property(x => x.Resumo).HasMaxLength(500);
        builder.Property(x => x.ImagemUrl).HasMaxLength(1000);
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasOne(x => x.CriadoPorUsuario).WithMany(x => x.TrilhasCriadas).HasForeignKey(x => x.CriadoPorUsuarioId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class TrilhaConteudoConfiguration : IEntityTypeConfiguration<TrilhaConteudo>
{
    public void Configure(EntityTypeBuilder<TrilhaConteudo> builder)
    {
        builder.ToTable("TrilhasConteudos");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TrilhaFormacaoId, x.ConteudoId }).IsUnique();
        builder.HasOne(x => x.TrilhaFormacao).WithMany(x => x.Conteudos).HasForeignKey(x => x.TrilhaFormacaoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Conteudo).WithMany(x => x.Trilhas).HasForeignKey(x => x.ConteudoId).OnDelete(DeleteBehavior.Cascade);
    }
}
