using OrchardCore.Modules.Services;

namespace OrchardCore.Localization;

public static class TransliterationSlugServiceExtensions
{
    /// <summary>
    /// Converts the specified text to a URL-friendly slug after applying transliteration.
    /// </summary>
    /// <param name="slugService">The slug service used to generate the slug from the transliterated text.</param>
    /// <param name="text">The input text to transliterate and convert to a slug. Cannot be null or empty.</param>
    /// <returns>A slugified string representing the transliterated input text.</returns>
    public static string SlugifyAndTransliterate(this ISlugService slugService, string text)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        return slugService.Slugify(text.Transliterate());
    }
}
