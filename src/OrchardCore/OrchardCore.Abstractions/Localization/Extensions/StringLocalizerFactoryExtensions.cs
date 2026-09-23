namespace Microsoft.Extensions.Localization;

/// <summary>
/// Provides extension methods for <see cref="IStringLocalizerFactory"/>.
/// </summary>
public static class StringLocalizerFactoryExtensions
{
    /// <summary>
    /// Translates a <see cref="LocalizedString"/> that was created with a translation context,
    /// for example with <c>LocalizedString.Create(name, typeof(MyType))</c>.
    /// </summary>
    /// <param name="factory">The <see cref="IStringLocalizerFactory"/>.</param>
    /// <param name="value">The <see cref="LocalizedString"/> to translate.</param>
    /// <returns>
    /// The translation of <see cref="LocalizedString.Name"/> in the context that <see cref="LocalizedString.SearchedLocation"/> contains.
    /// When <paramref name="value"/> is <see langword="null"/> or has no context, <paramref name="value"/> is returned.
    /// </returns>
    public static LocalizedString Localize(this IStringLocalizerFactory factory, LocalizedString value)
    {
        ArgumentNullException.ThrowIfNull(factory);

        if (value is null || string.IsNullOrEmpty(value.SearchedLocation))
        {
            return value;
        }

        return factory.Create(value.SearchedLocation, string.Empty)[value.Name];
    }
}
