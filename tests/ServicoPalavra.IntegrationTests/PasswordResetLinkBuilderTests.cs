using ServicoPalavra.Infrastructure.Email;

namespace ServicoPalavra.IntegrationTests;

public sealed class PasswordResetLinkBuilderTests
{
    [Fact]
    public void Build_creates_expected_frontend_reset_url()
    {
        var userId = Guid.Parse("11111111-2222-3333-4444-555555555555");

        var url = PasswordResetLinkBuilder.Build(
            "https://app.example/base/",
            userId,
            "abc-_123");

        Assert.Equal(
            "https://app.example/base/redefinir-senha?userId=11111111-2222-3333-4444-555555555555&token=abc-_123",
            url);
    }

    [Fact]
    public void Build_escapes_query_values()
    {
        var userId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var url = PasswordResetLinkBuilder.Build(
            "https://app.example",
            userId,
            "token with spaces&value");

        Assert.Contains("token=token%20with%20spaces%26value", url, StringComparison.Ordinal);
    }
}
