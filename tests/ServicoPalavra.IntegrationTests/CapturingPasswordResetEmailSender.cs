using System.Collections.Concurrent;
using ServicoPalavra.Application.Auth;

namespace ServicoPalavra.IntegrationTests;

public sealed record CapturedPasswordResetEmail(
    string Email,
    string Nome,
    Guid UserId,
    string EncodedToken);

public sealed class CapturingPasswordResetEmailSender : IPasswordResetEmailSender
{
    private readonly ConcurrentQueue<CapturedPasswordResetEmail> _messages = new();

    public IReadOnlyList<CapturedPasswordResetEmail> Messages => _messages.ToArray();

    public Task SendPasswordResetAsync(
        string email,
        string nome,
        Guid userId,
        string encodedToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _messages.Enqueue(new CapturedPasswordResetEmail(email, nome, userId, encodedToken));
        return Task.CompletedTask;
    }

    public void Clear()
    {
        while (_messages.TryDequeue(out _))
        {
        }
    }
}
