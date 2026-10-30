using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ServicoPalavra.Infrastructure.Persistence;

internal sealed class DataProtectionKeyStoreInitializer : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataProtectionKeyStoreInitializer> _logger;

    public DataProtectionKeyStoreInitializer(
        IServiceProvider serviceProvider,
        ILogger<DataProtectionKeyStoreInitializer> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DataProtectionDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("Data Protection key store ready using persistent database storage");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
