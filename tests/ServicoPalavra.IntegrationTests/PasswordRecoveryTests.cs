using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ServicoPalavra.Application.Auth;

namespace ServicoPalavra.IntegrationTests;

public sealed class PasswordRecoveryTests : IClassFixture<SecurityWebApplicationFactory>
{
    private readonly SecurityWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public PasswordRecoveryTests(SecurityWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _factory.PasswordResetEmailSender.Clear();
    }

    [Fact]
    public async Task Forgot_password_uses_generic_response_and_does_not_enumerate_accounts()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"recovery-{suffix}@tests.local";
        await RegisterAsync("Usuario Recovery", email, "StrongPass123");

        _factory.PasswordResetEmailSender.Clear();
        await EnsureCsrfAsync(_client);
        var existing = await _client.PostAsJsonAsync("/api/auth/password/forgot", new ForgotPasswordRequest(email));
        var existingBody = await existing.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, existing.StatusCode);
        var captured = await WaitForSingleCapturedAsync();
        Assert.Equal(email, captured.Email);
        Assert.Equal("Usuario Recovery", captured.Nome);
        Assert.NotEqual(Guid.Empty, captured.UserId);
        Assert.False(string.IsNullOrWhiteSpace(captured.EncodedToken));
        Assert.DoesNotContain("+", captured.EncodedToken);
        Assert.DoesNotContain("/", captured.EncodedToken);
        Assert.DoesNotContain("=", captured.EncodedToken);

        _factory.PasswordResetEmailSender.Clear();
        var missingEmail = $"missing-{suffix}@tests.local";
        var missing = await _client.PostAsJsonAsync("/api/auth/password/forgot", new ForgotPasswordRequest(missingEmail));
        var missingBody = await missing.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, missing.StatusCode);
        Assert.Equal(existingBody, missingBody);
        await Task.Delay(100);
        Assert.Empty(_factory.PasswordResetEmailSender.Messages);
        Assert.DoesNotContain(email, existingBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(missingEmail, missingBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Valid_reset_changes_password_and_token_cannot_be_reused()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"reset-{suffix}@tests.local";
        const string oldPassword = "StrongPass123";
        const string newPassword = "NovaSenhaSegura123";

        await RegisterAsync("Usuario Reset", email, oldPassword);
        _factory.PasswordResetEmailSender.Clear();

        await EnsureCsrfAsync(_client);
        var forgot = await _client.PostAsJsonAsync("/api/auth/password/forgot", new ForgotPasswordRequest(email));
        Assert.Equal(HttpStatusCode.OK, forgot.StatusCode);
        var captured = await WaitForSingleCapturedAsync();

        var reset = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(captured.UserId, captured.EncodedToken, newPassword));
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);

        var reuse = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(captured.UserId, captured.EncodedToken, "OutraSenhaSegura123"));
        Assert.Equal(HttpStatusCode.BadRequest, reuse.StatusCode);

        var oldLogin = await LoginWithFreshClientAsync(email, oldPassword);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);

        var newLogin = await LoginWithFreshClientAsync(email, newPassword);
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task Invalid_reset_token_and_unknown_user_use_same_public_error()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"invalid-reset-{suffix}@tests.local";
        await RegisterAsync("Usuario Token Invalido", email, "StrongPass123");

        _factory.PasswordResetEmailSender.Clear();
        await EnsureCsrfAsync(_client);
        var forgot = await _client.PostAsJsonAsync("/api/auth/password/forgot", new ForgotPasswordRequest(email));
        Assert.Equal(HttpStatusCode.OK, forgot.StatusCode);
        var captured = await WaitForSingleCapturedAsync();

        var invalidToken = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(captured.UserId, captured.EncodedToken + "alterado", "NovaSenhaSegura123"));
        var invalidTokenBody = await invalidToken.Content.ReadAsStringAsync();

        var unknownUser = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(Guid.NewGuid(), captured.EncodedToken, "NovaSenhaSegura123"));
        var unknownUserBody = await unknownUser.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, invalidToken.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unknownUser.StatusCode);
        Assert.Equal(invalidTokenBody, unknownUserBody);
        Assert.Contains("invalido ou expirado", invalidTokenBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Identity", invalidTokenBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PasswordHash", invalidTokenBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(captured.EncodedToken, invalidTokenBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Weak_new_password_returns_policy_error_and_keeps_valid_token_usable()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"weak-reset-{suffix}@tests.local";
        await RegisterAsync("Usuario Senha Fraca", email, "StrongPass123");

        _factory.PasswordResetEmailSender.Clear();
        await EnsureCsrfAsync(_client);
        var forgot = await _client.PostAsJsonAsync("/api/auth/password/forgot", new ForgotPasswordRequest(email));
        Assert.Equal(HttpStatusCode.OK, forgot.StatusCode);
        var captured = await WaitForSingleCapturedAsync();

        var weak = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(captured.UserId, captured.EncodedToken, "fraca"));
        var weakBody = await weak.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);
        Assert.Contains("requisitos de seguranca", weakBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("invalido ou expirado", weakBody, StringComparison.OrdinalIgnoreCase);

        var valid = await _client.PostAsJsonAsync(
            "/api/auth/password/reset",
            new ResetPasswordRequest(captured.UserId, captured.EncodedToken, "AgoraSim123"));
        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
    }

    [Fact]
    public async Task Registration_accepts_strong_password_longer_than_eight_characters()
    {
        var suffix = Guid.NewGuid().ToString("N");
        var email = $"long-password-{suffix}@tests.local";
        await EnsureCsrfAsync(_client);

        var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("Senha Longa", email, "UmaSenhaLonga123"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<CapturedPasswordResetEmail> WaitForSingleCapturedAsync()
    {
        for (var attempt = 0; attempt < 40; attempt++)
        {
            var messages = _factory.PasswordResetEmailSender.Messages;
            if (messages.Count == 1)
            {
                return messages[0];
            }

            await Task.Delay(25);
        }

        return Assert.Single(_factory.PasswordResetEmailSender.Messages);
    }

    private async Task RegisterAsync(string nome, string email, string password)
    {
        await EnsureCsrfAsync(_client);
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(nome, email, password));
        response.EnsureSuccessStatusCode();
        await EnsureCsrfAsync(_client);
    }

    private static async Task EnsureCsrfAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/csrf");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var token = document.RootElement.GetProperty("data").GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token);
    }

    private async Task<HttpResponseMessage> LoginWithFreshClientAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        await EnsureCsrfAsync(client);
        return await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
    }
}
