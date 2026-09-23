namespace Microsoft.Extensions.Localization;

/// <summary>
/// Provides extension members for <see cref="LocalizedString"/>.
/// </summary>
public static class LocalizedStringExtensions
{
    extension(LocalizedString)
    {
        /// <summary>
        /// Creates a <see cref="LocalizedString"/> that uses the same text for its name and its value.
        /// </summary>
        /// <param name="name">The name of the string. It is also used as the value.</param>
        /// <returns>A <see cref="LocalizedString"/> where <see cref="LocalizedString.Name"/> and <see cref="LocalizedString.Value"/> are equal to <paramref name="name"/>.</returns>
        /// <remarks>
        /// The returned string is not translated. The name can be used as the key to localize the string later.
        /// </remarks>
        public static LocalizedString Create(string name)
            => new(name, name);
    }
}
