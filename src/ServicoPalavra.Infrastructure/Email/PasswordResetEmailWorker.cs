using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServicoPalavra.Application.Auth;

namespace ServicoPalavra.Infrastructure.Email;

public sealed class PasswordResetEmailWorker : BackgroundService
{
    private readonly IPasswordResetEmailQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PasswordResetEmailWorker> _logger;

    public PasswordResetEmailWorker(
        IPasswordResetEmailQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<PasswordResetEmailWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<IPasswordResetEmailSender>();
                await sender.SendPasswordResetAsync(
                    job.Email,
                    job.Nome,
                    job.UserId,
                    job.EncodedToken,
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Falha ao processar email de recuperacao de senha em background.");
            }
        }
    }
}
