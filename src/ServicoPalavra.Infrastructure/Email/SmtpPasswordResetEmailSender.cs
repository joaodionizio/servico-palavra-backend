using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServicoPalavra.Application.Auth;

namespace ServicoPalavra.Infrastructure.Email;

public sealed class SmtpPasswordResetEmailSender : IPasswordResetEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpPasswordResetEmailSender> _logger;

    public SmtpPasswordResetEmailSender(
        IConfiguration configuration,
        ILogger<SmtpPasswordResetEmailSender> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetAsync(
        string email,
        string nome,
        Guid userId,
        string encodedToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var frontendBaseUrl = Read("Email:PasswordReset:FrontendBaseUrl", "FRONTEND_BASE_URL");
        var host = Read("Email:Smtp:Host", "SMTP_HOST");
        var fromAddress = Read("Email:Smtp:FromAddress", "SMTP_FROM_ADDRESS");

        if (string.IsNullOrWhiteSpace(frontendBaseUrl)
            || string.IsNullOrWhiteSpace(host)
            || string.IsNullOrWhiteSpace(fromAddress))
        {
            _logger.LogWarning("Recuperacao de senha solicitada, mas SMTP/FRONTEND_BASE_URL nao estao configurados.");
            return;
        }

        var resetUrl = BuildResetUrl(frontendBaseUrl, userId, encodedToken);
        var fromName = Read("Email:Smtp:FromName", "SMTP_FROM_NAME") ?? "Servico da Palavra";
        var userName = Read("Email:Smtp:UserName", "SMTP_USERNAME");
        var password = Read("Email:Smtp:Password", "SMTP_PASSWORD");
        var port = ReadInt("Email:Smtp:Port", "SMTP_PORT", 587);
        var enableSsl = ReadBool("Email:Smtp:EnableSsl", "SMTP_ENABLE_SSL", true);

        try
        {
            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName, Encoding.UTF8),
                Subject = "Redefinicao de senha - Servico da Palavra",
                SubjectEncoding = Encoding.UTF8,
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = true,
                Body = BuildBody(nome, resetUrl)
            };
            message.To.Add(new MailAddress(email));

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };

            if (!string.IsNullOrWhiteSpace(userName))
            {
                client.Credentials = new NetworkCredential(userName, password ?? string.Empty);
            }

            await client.SendMailAsync(message);
        }
        catch (Exception exception) when (exception is SmtpException or InvalidOperationException or FormatException)
        {
            _logger.LogError(exception, "Falha ao enviar email de recuperacao de senha.");
        }
    }

    private string? Read(string configKey, string envKey) =>
        _configuration[configKey] ?? _configuration[envKey];

    private int ReadInt(string configKey, string envKey, int fallback) =>
        int.TryParse(Read(configKey, envKey), out var value) ? value : fallback;

    private bool ReadBool(string configKey, string envKey, bool fallback) =>
        bool.TryParse(Read(configKey, envKey), out var value) ? value : fallback;

    private static string BuildResetUrl(string frontendBaseUrl, Guid userId, string encodedToken)
    {
        var baseUrl = frontendBaseUrl.TrimEnd('/');
        return $"{baseUrl}/redefinir-senha?userId={Uri.EscapeDataString(userId.ToString())}&token={Uri.EscapeDataString(encodedToken)}";
    }

    private static string BuildBody(string nome, string resetUrl)
    {
        var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(nome) ? "pessoa" : nome);
        var safeUrl = WebUtility.HtmlEncode(resetUrl);

        return $$"""
            <div style="font-family:Arial,sans-serif;line-height:1.6;color:#1f2937">
              <h2 style="color:#004b87">Servico da Palavra</h2>
              <p>Ola, {{safeName}}.</p>
              <p>Recebemos uma solicitacao para redefinir a senha da sua conta.</p>
              <p><a href="{{safeUrl}}" style="display:inline-block;padding:12px 20px;border-radius:999px;background:#004b87;color:white;text-decoration:none;font-weight:700">Redefinir senha</a></p>
              <p>Este link e temporario. Se voce nao solicitou a alteracao, ignore esta mensagem.</p>
            </div>
            """;
    }
}
