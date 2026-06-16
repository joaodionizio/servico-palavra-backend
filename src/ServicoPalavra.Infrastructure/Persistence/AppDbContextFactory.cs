using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ServicoPalavra.Infrastructure.Persistence;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        var provider = (configuration["DATABASE_PROVIDER"] ?? "sqlite").Trim().ToLowerInvariant();
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration["CONNECTION_STRING"];

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        if (provider is "postgresql" or "postgres")
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:DefaultConnection deve ser configurada para DATABASE_PROVIDER=postgresql.");
            }

            optionsBuilder.UseNpgsql(connectionString);
        }
        else if (provider == "sqlite")
        {
            optionsBuilder.UseSqlite(string.IsNullOrWhiteSpace(connectionString) ? "Data Source=servico-palavra.db" : connectionString);
        }
        else
        {
            throw new InvalidOperationException("DATABASE_PROVIDER deve ser 'sqlite' ou 'postgresql'.");
        }

        return new AppDbContext(optionsBuilder.Options);
    }
}
