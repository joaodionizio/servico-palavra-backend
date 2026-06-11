using ServicoPalavra.Domain.Enums;

namespace ServicoPalavra.Application.Common;

public static class ExternalUrlValidator
{
    private static readonly HashSet<string> YouTubeHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "youtube.com",
        "www.youtube.com",
        "youtu.be",
        "m.youtube.com"
    };

    private static readonly HashSet<string> GoogleDriveHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "drive.google.com",
        "docs.google.com"
    };

    public static void ValidateConteudoUrl(string url, OrigemConteudo origem)
    {
        var uri = ParseHttpsUri(url, "URL do conteudo invalida.");

        if (origem == OrigemConteudo.YouTube && !YouTubeHosts.Contains(uri.Host))
        {
            throw new AppException("Conteudos do YouTube devem usar dominio youtube.com ou youtu.be.");
        }

        if (origem == OrigemConteudo.GoogleDrive && !GoogleDriveHosts.Contains(uri.Host))
        {
            throw new AppException("Conteudos do Google Drive devem usar dominio drive.google.com ou docs.google.com.");
        }
    }

    public static void ValidateOptionalThumbnail(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        ParseHttpsUri(url, "URL de thumbnail invalida.");
    }

    private static Uri ParseHttpsUri(string url, string errorMessage)
    {
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new AppException(errorMessage);
        }

        return uri;
    }
}
