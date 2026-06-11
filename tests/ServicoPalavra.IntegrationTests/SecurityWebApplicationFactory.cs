using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ServicoPalavra.IntegrationTests;

public sealed class SecurityWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"servico-palavra-security-{Guid.NewGuid():N}.db");

    public SecurityWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("DATABASE_PROVIDER", "sqlite");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", $"Data Source={_databasePath}");
        Environment.SetEnvironmentVariable("INITIAL_ADMIN_EMAIL", "admin@tests.local");
        Environment.SetEnvironmentVariable("INITIAL_ADMIN_PASSWORD", "AdminTest@123456");
        Environment.SetEnvironmentVariable("INITIAL_ADMIN_NAME", "Admin Tests");
    }

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
                ["INITIAL_ADMIN_PASSWORD"] = "AdminTest@123456",
                ["INITIAL_ADMIN_NAME"] = "Admin Tests"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Environment.SetEnvironmentVariable("DATABASE_PROVIDER", null);
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", null);
        Environment.SetEnvironmentVariable("INITIAL_ADMIN_EMAIL", null);
        Environment.SetEnvironmentVariable("INITIAL_ADMIN_PASSWORD", null);
        Environment.SetEnvironmentVariable("INITIAL_ADMIN_NAME", null);
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
