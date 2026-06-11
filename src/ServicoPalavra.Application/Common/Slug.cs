using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ServicoPalavra.Application.Common;

public static partial class Slug
{
    public static string From(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        var clean = NonAlphaNumericRegex().Replace(builder.ToString().Normalize(NormalizationForm.FormC), "-");
        return DuplicateDashRegex().Replace(clean, "-").Trim('-');
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex("-+")]
    private static partial Regex DuplicateDashRegex();
}
