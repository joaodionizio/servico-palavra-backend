using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServicoPalavra.Domain.Entities;

namespace ServicoPalavra.Infrastructure.Persistence.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("Usuarios");
        builder.Property(x => x.Nome).HasMaxLength(160).IsRequired();
        builder.Property(x => x.FotoUrl).HasMaxLength(600);
        builder.Property(x => x.Email).HasMaxLength(180).IsRequired();
    }
}
