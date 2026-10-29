using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ServicoPalavra.Application.Auth;

namespace ServicoPalavra.IntegrationTests;

public sealed class PasswordRecoveryTimingTests : IClassFixture<SecurityWebApplicationFactory>
{
    private readonly SecurityWebApplicationFactory _factory;

    public PasswordRecoveryTimingTests(SecurityWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Forgot_password_returns_while_email_sender_is_still_blocked()
    {
        var client = _factory.CreateClient();
        var email = $"timing-{Guid.NewGuid():N}@tests.local";
        await EnsureCsrfAsync(client);

        var register = await client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest("Timing Test", email, "StrongPass123"));
        register.EnsureSuccessStatusCode();

        _factory.PasswordResetEmailSender.Clear();
        _factory.PasswordResetEmailSender.Block();

        try
        {
            await EnsureCsrfAsync(client);
            var requestTask = client.PostAsJsonAsync(
                "/api/auth/password/forgot",
                new ForgotPasswordRequest(email));

            await _factory.PasswordResetEmailSender.Started.WaitAsync(TimeSpan.FromSeconds(2));
            var response = await requestTask.WaitAsync(TimeSpan.FromSeconds(2));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(_factory.PasswordResetEmailSender.Messages);
        }
        finally
        {
            _factory.PasswordResetEmailSender.Release();
        }
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
}
