using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence.Configurations;

public sealed class PerfilConfiguration : IEntityTypeConfiguration<Perfil>
{
    public void Configure(EntityTypeBuilder<Perfil> builder)
    {
        builder.ToTable("Perfis");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(300);
        builder.HasIndex(x => x.Nome).IsUnique();
        builder.Ignore(x => x.Usuarios);
    }
}
