using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Infrastructure.Persistence;
using ServicoPalavra.Infrastructure.Persistence.Import;
using ServicoPalavra.Infrastructure.Persistence.Seed;

namespace ServicoPalavra.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            var provider = (configuration["DATABASE_PROVIDER"] ?? "sqlite").Trim().ToLowerInvariant();
            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration["CONNECTION_STRING"];

            if (provider is "postgresql" or "postgres")
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("ConnectionStrings:DefaultConnection deve ser configurada para DATABASE_PROVIDER=postgresql.");
                }

                options.UseNpgsql(connectionString);
                return;
            }

            if (provider == "sqlite")
            {
                options.UseSqlite(string.IsNullOrWhiteSpace(connectionString) ? "Data Source=servico-palavra-dev.db" : connectionString);
                return;
            }

            throw new InvalidOperationException("DATABASE_PROVIDER deve ser 'sqlite' ou 'postgresql'.");
        });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IConteudoRepository, ConteudoRepository>();
        services.AddScoped<IFavoritoRepository, FavoritoRepository>();
        services.AddScoped<IProgressoRepository, ProgressoRepository>();
        services.AddScoped<ITrilhaRepository, TrilhaRepository>();
        services.AddScoped<IPlanoBiblicoRepository, PlanoBiblicoRepository>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<BaseBiblicaV2Importer>();

        return services;
    }
}
