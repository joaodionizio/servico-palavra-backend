namespace ServicoPalavra.Infrastructure.Email;

public static class PasswordResetLinkBuilder
{
    public static string Build(string frontendBaseUrl, Guid userId, string encodedToken)
    {
        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            throw new ArgumentException("Frontend base URL is required.", nameof(frontendBaseUrl));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(encodedToken))
        {
            throw new ArgumentException("Reset token is required.", nameof(encodedToken));
        }

        var baseUrl = frontendBaseUrl.TrimEnd('/');
        return $"{baseUrl}/redefinir-senha?userId={Uri.EscapeDataString(userId.ToString())}&token={Uri.EscapeDataString(encodedToken)}";
    }
}
