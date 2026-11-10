using OrchardCore.Localization;

namespace Microsoft.AspNetCore.Mvc.Localization;

/// <summary>
/// Provides extension methods for <see cref="IHtmlLocalizer"/>.
/// </summary>
public static class HtmlLocalizerExtensions
{
    /// <summary>
    /// Gets the pluralization form.
    /// </summary>
    /// <param name="localizer">The <see cref="IHtmlLocalizer"/>.</param>
    /// <param name="count">The number to be used for selecting the pluralization form.</param>
    /// <param name="singular">The singular form key.</param>
    /// <param name="plural">The plural form key.</param>
    /// <param name="arguments">The parameters used in the key.</param>
    [SkipLocalizationExtraction]
    public static LocalizedHtmlString Plural(this IHtmlLocalizer localizer, int count, string singular, string plural, params object[] arguments)
    {
        ArgumentNullException.ThrowIfNull(plural);

        return localizer[singular, new PluralizationArgument { Count = count, Forms = [singular, plural], Arguments = arguments }];
    }

    /// <summary>
    /// Gets the pluralization form.
    /// </summary>
    /// <param name="localizer">The <see cref="IHtmlLocalizer"/>.</param>
    /// <param name="count">The number to be used for selecting the pluralization form.</param>
    /// <param name="pluralForms">A list of pluralization forms.</param>
    /// <param name="arguments">The parameters used in the key.</param>
    [SkipLocalizationExtraction]
    public static LocalizedHtmlString Plural(this IHtmlLocalizer localizer, int count, string[] pluralForms, params object[] arguments)
    {
        ArgumentNullException.ThrowIfNull(pluralForms);

        if (pluralForms.Length == 0)
        {
            throw new ArgumentException("PluralForms array can't be empty, it must contain at least one element. If you don't want to specify the plural text, use IStringLocalizer without Plural extension.", nameof(pluralForms));
        }

        var name = pluralForms[0];

        return localizer[name, new PluralizationArgument { Count = count, Forms = pluralForms, Arguments = arguments }];
    }

    /// <summary>
    /// Attaches a note for translators to the strings localized with the returned localizer, for example
    /// <c>localizer.Note("Shown in the toolbar.")["Save"]</c>. The note is read by the localization extraction and
    /// written to the template; it has no effect at runtime and does not change the translation lookup.
    /// </summary>
    /// <typeparam name="TLocalizer">The type of the localizer.</typeparam>
    /// <param name="localizer">The localizer.</param>
    /// <param name="note">The note explaining the meaning or usage of the message. Must be a compile-time constant to be extracted.</param>
    /// <returns><paramref name="localizer"/>, unchanged.</returns>
    public static TLocalizer Note<TLocalizer>(this TLocalizer localizer, string note)
        where TLocalizer : IHtmlLocalizer
    {
        _ = note;

        return localizer;
    }

    /// <summary>
    /// Gets the pluralization form, with a note for translators.
    /// </summary>
    /// <param name="localizer">The <see cref="IHtmlLocalizer"/>.</param>
    /// <param name="note">The note explaining the meaning or usage of the message to translators. It is read by the localization extraction and has no effect at runtime.</param>
    /// <param name="count">The number to be used for selecting the pluralization form.</param>
    /// <param name="singular">The singular form key.</param>
    /// <param name="plural">The plural form key.</param>
    /// <param name="arguments">The parameters used in the key.</param>
    [SkipLocalizationExtraction]
    public static LocalizedHtmlString Plural(this IHtmlLocalizer localizer, string note, int count, string singular, string plural, params object[] arguments)
    {
        _ = note;

        return localizer.Plural(count, singular, plural, arguments);
    }

    /// <summary>
    /// Gets the pluralization form, with a note for translators.
    /// </summary>
    /// <param name="localizer">The <see cref="IHtmlLocalizer"/>.</param>
    /// <param name="note">The note explaining the meaning or usage of the message to translators. It is read by the localization extraction and has no effect at runtime.</param>
    /// <param name="count">The number to be used for selecting the pluralization form.</param>
    /// <param name="pluralForms">A list of pluralization forms.</param>
    /// <param name="arguments">The parameters used in the key.</param>
    [SkipLocalizationExtraction]
    public static LocalizedHtmlString Plural(this IHtmlLocalizer localizer, string note, int count, string[] pluralForms, params object[] arguments)
    {
        _ = note;

        return localizer.Plural(count, pluralForms, arguments);
    }
}
