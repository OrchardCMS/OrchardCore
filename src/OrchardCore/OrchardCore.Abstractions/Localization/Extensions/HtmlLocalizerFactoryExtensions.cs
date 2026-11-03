using OrchardCore.Localization;

namespace Microsoft.AspNetCore.Mvc.Localization;

/// <summary>
/// Provides extension methods for <see cref="IHtmlLocalizerFactory"/>.
/// </summary>
public static class HtmlLocalizerFactoryExtensions
{
    /// <summary>
    /// Translates a localization source with its source type as the context.
    /// </summary>
    /// <param name="factory">The <see cref="IHtmlLocalizerFactory"/>.</param>
    /// <param name="source">The localization source to translate.</param>
    /// <param name="arguments">The values to format the translation with. Arguments are HTML encoded when rendered.</param>
    /// <returns>
    /// The translated HTML string, or the source value when no source type is provided.
    /// Returns <see langword="null"/> when <paramref name="source"/> is <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// The translation itself is not HTML encoded. Use a string localizer for text that must be encoded when rendered.
    /// </remarks>
    [SkipLocalizationExtraction]
    public static LocalizedHtmlString Localize(this IHtmlLocalizerFactory factory, LocalizationSource source, params object[] arguments)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (source is null)
        {
            return null;
        }

        if (source.Type is null)
        {
            return arguments is { Length: > 0 }
                ? new LocalizedHtmlString(source.Value, source.Value, false, arguments)
                : new LocalizedHtmlString(source.Value, source.Value);
        }

        var localizer = factory.Create(source.Type);

        return arguments is { Length: > 0 }
            ? localizer[source.Value, arguments]
            : localizer[source.Value];
    }
}
