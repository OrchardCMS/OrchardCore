using OrchardCore.Localization;

namespace Microsoft.AspNetCore.Mvc.Localization;

/// <summary>
/// Provides pluralization extension methods for <see cref="IHtmlLocalizerFactory"/> that resolve a localizer
/// from a <see cref="LocalizationSource"/>.
/// </summary>
public static class HtmlLocalizerFactoryPluralExtensions
{
    /// <summary>
    /// Gets the pluralization form of a localization source with its source type as the context.
    /// </summary>
    /// <param name="factory">The <see cref="IHtmlLocalizerFactory"/>.</param>
    /// <param name="count">The number to be used for selecting the pluralization form. Automatically passed as the first format argument (<c>{0}</c>).</param>
    /// <param name="source">The singular localization source to translate.</param>
    /// <param name="plural">The plural form text.</param>
    /// <param name="arguments">The additional values to format the translation with, starting at <c>{1}</c>. Arguments are HTML encoded when rendered.</param>
    /// <returns>
    /// The translated HTML string in the pluralization form selected by the culture's PO rules, or the English
    /// singular/plural form of <paramref name="source"/> when no source type is provided.
    /// Returns <see langword="null"/> when <paramref name="source"/> is <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// The translation itself is not HTML encoded. Use a string localizer for text that must be encoded when rendered.
    /// </remarks>
    public static LocalizedHtmlString Plural(this IHtmlLocalizerFactory factory, int count, LocalizationSource source, string plural, params object[] arguments)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (source is null)
        {
            return null;
        }

        ArgumentNullException.ThrowIfNull(plural);

        if (source.Type is null)
        {
            var (text, formatArguments) = LocalizationSourcePluralHelper.SelectContextFreeForm(source, count, plural, arguments);

            return new LocalizedHtmlString(source.Value, text, false, formatArguments);
        }

        var localizer = factory.Create(source.Type);

        return localizer.Plural(count, source.Value, plural, arguments ?? []);
    }
}
