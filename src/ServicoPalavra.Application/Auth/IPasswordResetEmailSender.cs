namespace ServicoPalavra.Application.Auth;

public interface IPasswordResetEmailSender
{
    Task SendPasswordResetAsync(
        string email,
        string nome,
        Guid userId,
        string encodedToken,
        CancellationToken cancellationToken = default);
}
