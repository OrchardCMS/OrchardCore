namespace Microsoft.Extensions.Localization;

/// <summary>
/// Provides extension members for <see cref="LocalizedString"/>.
/// </summary>
public static class LocalizedStringExtensions
{
    extension(LocalizedString)
    {
        /// <summary>
        /// Creates a <see cref="LocalizedString"/> from a value. The value is also used as the name.
        /// </summary>
        /// <param name="value">The value of the string. It is also used as the name.</param>
        /// <param name="arguments">The values to format the <paramref name="value"/> with.</param>
        /// <returns>
        /// A <see cref="LocalizedString"/> where <see cref="LocalizedString.Name"/> is <paramref name="value"/>, and
        /// <see cref="LocalizedString.Value"/> is <paramref name="value"/> formatted with <paramref name="arguments"/>.
        /// When there are no arguments, <paramref name="value"/> is not formatted.
        /// </returns>
        /// <remarks>
        /// The returned string is not translated. The name can be used as the key to localize the string later.
        /// </remarks>
        public static LocalizedString Create(string value, params object[] arguments)
        {
            ArgumentNullException.ThrowIfNull(value);

            return new(value, arguments is { Length: > 0 } ? string.Format(value, arguments) : value);
        }

        /// <summary>
        /// Creates a <see cref="LocalizedString"/> from a value, and keeps the type to use as the translation context.
        /// The value is also used as the name.
        /// </summary>
        /// <param name="value">The value of the string. It is also used as the name.</param>
        /// <param name="resourceSource">The type to use as the translation context, usually the type that declares the string.</param>
        /// <returns>A <see cref="LocalizedString"/> where <see cref="LocalizedString.Name"/> and <see cref="LocalizedString.Value"/> are equal to <paramref name="value"/>,
        /// and <see cref="LocalizedString.SearchedLocation"/> is the full name of <paramref name="resourceSource"/>.</returns>
        /// <remarks>
        /// The returned string is not translated. Use <see cref="StringLocalizerFactoryExtensions.Localize(IStringLocalizerFactory, LocalizedString)"/>
        /// to translate it later with the same context as <see cref="IStringLocalizer{T}"/> of <paramref name="resourceSource"/>.
        /// </remarks>
        public static LocalizedString Create(string value, Type resourceSource)
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentNullException.ThrowIfNull(resourceSource);

            return new(value, value, resourceNotFound: false, searchedLocation: resourceSource.FullName);
        }
    }
}
