using OrchardCore.Modules.Services;

namespace OrchardCore.Localization;

public static class TransliterationSlugServiceExtensions
{
    /// <summary>
    /// Transforms specified text to the form suitable for URL slugs,
    /// optionally transliterating non-Latin characters to their ASCII equivalents first.
    /// </summary>
    /// <param name="text">The text to transform.</param>
    /// <returns>The slug created from the input text.</returns>
    public static string SlugifyWithTransliteration(this ISlugService slugService, string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        return slugService.Slugify(text.Transliterate());
    }
}
