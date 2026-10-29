namespace ServicoPalavra.Application.Auth;

public sealed record PasswordResetEmailJob(
    string Email,
    string Nome,
    Guid UserId,
    string EncodedToken);

public interface IPasswordResetEmailQueue
{
    bool TryEnqueue(PasswordResetEmailJob job);
    IAsyncEnumerable<PasswordResetEmailJob> ReadAllAsync(CancellationToken cancellationToken = default);
}
