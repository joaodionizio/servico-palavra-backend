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
    private TaskCompletionSource<bool>? _releaseGate;
    private TaskCompletionSource<bool>? _startedGate;

    public IReadOnlyList<CapturedPasswordResetEmail> Messages => _messages.ToArray();

    public Task Started => _startedGate?.Task ?? Task.CompletedTask;

    public async Task SendPasswordResetAsync(
        string email,
        string nome,
        Guid userId,
        string encodedToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _messages.Enqueue(new CapturedPasswordResetEmail(email, nome, userId, encodedToken));
        _startedGate?.TrySetResult(true);

        var releaseGate = _releaseGate;
        if (releaseGate is not null)
        {
            await releaseGate.Task.WaitAsync(cancellationToken);
        }
    }

    public void Block()
    {
        _startedGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        _releaseGate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public void Release()
    {
        _releaseGate?.TrySetResult(true);
        _releaseGate = null;
        _startedGate = null;
    }

    public void Clear()
    {
        Release();
        while (_messages.TryDequeue(out _))
        {
        }
    }
}
