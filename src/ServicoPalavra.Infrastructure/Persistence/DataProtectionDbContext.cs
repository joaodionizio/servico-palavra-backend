using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class DataProtectionDbContext : DbContext, IDataProtectionKeyContext
{
    public DataProtectionDbContext(DbContextOptions<DataProtectionDbContext> options) : base(options)
    {
    }

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();
}
