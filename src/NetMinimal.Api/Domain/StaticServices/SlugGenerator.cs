using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NetMinimal.Domain.StaticServices;

public static partial class SlugGenerator
{
    public static string From(string value)
    {
        var source = Guard.AgainstNullOrWhiteSpace(value).ToLowerInvariant();
        var normalized = source.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        var slug = NonSlugChars().Replace(builder.ToString().Normalize(NormalizationForm.FormC), "-");
        slug = CollapseDashes().Replace(slug, "-").Trim('-');
        return slug.Length == 0 ? "item" : slug;
    }

    [GeneratedRegex("[^a-z0-9]+", RegexOptions.CultureInvariant)]
    private static partial Regex NonSlugChars();

    [GeneratedRegex("-{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex CollapseDashes();
}
