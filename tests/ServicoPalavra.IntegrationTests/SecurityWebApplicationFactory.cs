using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServicoPalavra.Application.Auth;

namespace ServicoPalavra.IntegrationTests;

public sealed class SecurityWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"servico-palavra-security-{Guid.NewGuid():N}.db");

    public CapturingPasswordResetEmailSender PasswordResetEmailSender { get; } = new();
    public DashboardCommandCounterInterceptor DashboardCommandCounter { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["DATABASE_PROVIDER"] = "sqlite",
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databasePath}",
                ["INITIAL_ADMIN_EMAIL"] = "admin@tests.local",
                ["INITIAL_ADMIN_PASSWORD"] = "Admin12",
                ["INITIAL_ADMIN_NAME"] = "Admin Tests"
            });
        });
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPasswordResetEmailSender>();
            services.AddSingleton<IPasswordResetEmailSender>(PasswordResetEmailSender);
            services.AddSingleton<IInterceptor>(DashboardCommandCounter);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        TryDelete(_databasePath);
        TryDelete(_databasePath + "-shm");
        TryDelete(_databasePath + "-wal");
    }

    private static void TryDelete(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
