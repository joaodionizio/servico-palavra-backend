using ServicoPalavra.Application.Common;
using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.UnitTests;

public sealed class ExternalUrlValidatorTests
{
    [Fact]
    public void Rejects_javascript_url()
    {
        Assert.Throws<AppException>(() => ExternalUrlValidator.ValidateConteudoUrl("javascript:alert(1)", OrigemConteudo.Externo));
    }

    [Fact]
    public void Rejects_wrong_host_for_youtube_origin()
    {
        Assert.Throws<AppException>(() => ExternalUrlValidator.ValidateConteudoUrl("https://example.com/watch?v=abc", OrigemConteudo.YouTube));
    }

    [Fact]
    public void Accepts_youtube_https_url()
    {
        ExternalUrlValidator.ValidateConteudoUrl("https://www.youtube.com/watch?v=abc", OrigemConteudo.YouTube);
    }
}
