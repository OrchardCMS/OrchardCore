using OrchardCore.Modules.Services;

namespace OrchardCore.Localization;

public static class TransliterationSlugServiceExtensions
{
    /// <summary>
    /// Transforms specified text to the form suitable for URL slugs,
    /// optionally transliterating non-Latin characters to their ASCII equivalents first.
    /// </summary>
    /// <param name="text">The text to transform.</param>
    /// <param name="transliterate">Whether to transliterate non-Latin characters before slugifying.</param>
    /// <returns>The slug created from the input text.</returns>
    public static string Slugify(this ISlugService slugService, string text, bool transliterate)
    {
        ArgumentException.ThrowIfNullOrEmpty(text);

        if (transliterate)
        {
            text = text.Transliterate();
        }

        return slugService.Slugify(text);
    }
}
