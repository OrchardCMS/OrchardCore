using OrchardCore.Localization;

namespace Microsoft.Extensions.Localization;

/// <summary>
/// Provides extension methods for <see cref="IStringLocalizerFactory"/>.
/// </summary>
public static class StringLocalizerFactoryExtensions
{
    /// <summary>
    /// Translates a localization source with its source type as the context.
    /// </summary>
    /// <param name="factory">The <see cref="IStringLocalizerFactory"/>.</param>
    /// <param name="source">The localization source to translate.</param>
    /// <param name="arguments">The values to format the translation with.</param>
    /// <returns>
    /// The translated string, or the source value when no source type is provided.
    /// Returns <see langword="null"/> when <paramref name="source"/> is <see langword="null"/>.
    /// </returns>
    public static LocalizedString Localize(this IStringLocalizerFactory factory, LocalizationSource source, params object[] arguments)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (source is null)
        {
            return null;
        }

        if (source.Type is null)
        {
            return new LocalizedString(source.Value, arguments is { Length: > 0 } ? string.Format(source.Value, arguments) : source.Value);
        }

        var localizer = factory.Create(source.Type);

        return arguments is { Length: > 0 }
            ? localizer[source.Value, arguments]
            : localizer[source.Value];
    }
}
