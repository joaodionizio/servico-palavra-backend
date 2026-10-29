using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using ServicoPalavra.Application.Auth;

namespace ServicoPalavra.Infrastructure.Email;

public sealed class PasswordResetEmailQueue : IPasswordResetEmailQueue
{
    private const int Capacity = 100;
    private readonly Channel<PasswordResetEmailJob> _channel;
    private readonly ILogger<PasswordResetEmailQueue> _logger;

    public PasswordResetEmailQueue(ILogger<PasswordResetEmailQueue> logger)
    {
        _logger = logger;
        _channel = Channel.CreateBounded<PasswordResetEmailJob>(new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
    }

    public bool TryEnqueue(PasswordResetEmailJob job)
    {
        var accepted = _channel.Writer.TryWrite(job);
        if (!accepted)
        {
            _logger.LogWarning("Fila de recuperacao de senha cheia; solicitacao de email nao foi enfileirada.");
        }

        return accepted;
    }

    public IAsyncEnumerable<PasswordResetEmailJob> ReadAllAsync(CancellationToken cancellationToken = default) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
