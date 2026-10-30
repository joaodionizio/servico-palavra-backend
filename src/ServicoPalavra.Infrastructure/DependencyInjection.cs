using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServicoPalavra.Application.Abstractions;
using ServicoPalavra.Application.Auth;
using ServicoPalavra.Infrastructure.Email;
using ServicoPalavra.Infrastructure.Persistence;
using ServicoPalavra.Infrastructure.Persistence.Import;
using ServicoPalavra.Infrastructure.Persistence.Seed;
using ServicoPalavra.Infrastructure.Time;

namespace ServicoPalavra.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = (configuration["DATABASE_PROVIDER"] ?? "sqlite").Trim().ToLowerInvariant();
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? configuration["CONNECTION_STRING"];

        void ConfigureDatabase(DbContextOptionsBuilder options)
        {
            if (provider is "postgresql" or "postgres")
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("ConnectionStrings:DefaultConnection deve ser configurada para DATABASE_PROVIDER=postgresql.");
                }

                options.UseNpgsql(connectionString);
            }
            else if (provider == "sqlite")
            {
                options.UseSqlite(string.IsNullOrWhiteSpace(connectionString) ? "Data Source=servico-palavra-dev.db" : connectionString);
            }
            else
            {
                throw new InvalidOperationException("DATABASE_PROVIDER deve ser 'sqlite' ou 'postgresql'.");
            }
        }

        services.AddDbContext<AppDbContext>((serviceProvider, options) =>
        {
            ConfigureDatabase(options);

            var interceptors = serviceProvider.GetServices<IInterceptor>().ToArray();
            if (interceptors.Length > 0)
            {
                options.AddInterceptors(interceptors);
            }
        });

        services.AddDbContext<DataProtectionDbContext>(ConfigureDatabase);

        var dataProtection = services
            .AddDataProtection()
            .PersistKeysToDbContext<DataProtectionDbContext>()
            .SetApplicationName("ServicoPalavra");

        var certificateBase64 = configuration["DATA_PROTECTION_CERTIFICATE_BASE64"];
        var certificatePassword = configuration["DATA_PROTECTION_CERTIFICATE_PASSWORD"];
        if (!string.IsNullOrWhiteSpace(certificateBase64))
        {
            if (string.IsNullOrWhiteSpace(certificatePassword))
            {
                throw new InvalidOperationException("DATA_PROTECTION_CERTIFICATE_PASSWORD deve ser configurada quando DATA_PROTECTION_CERTIFICATE_BASE64 estiver presente.");
            }

            var certificate = X509CertificateLoader.LoadPkcs12(
                Convert.FromBase64String(certificateBase64),
                certificatePassword,
                X509KeyStorageFlags.EphemeralKeySet);
            dataProtection.ProtectKeysWithCertificate(certificate);
        }

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ICalendarClock, CalendarClock>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IConteudoRepository, ConteudoRepository>();
        services.AddScoped<IFavoritoRepository, FavoritoRepository>();
        services.AddScoped<IProgressoRepository, ProgressoRepository>();
        services.AddScoped<IPlanoBiblicoRepository, PlanoBiblicoRepository>();
        services.AddScoped<IDashboardReadRepository, DashboardReadRepository>();
        services.AddScoped<IPasswordResetEmailSender, SmtpPasswordResetEmailSender>();
        services.AddSingleton<IPasswordResetEmailQueue, PasswordResetEmailQueue>();
        services.AddHostedService<PasswordResetEmailWorker>();
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<BaseBiblicaV2Importer>();

        return services;
    }
}
