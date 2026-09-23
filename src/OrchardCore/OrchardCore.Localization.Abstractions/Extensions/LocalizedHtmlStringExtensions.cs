namespace Microsoft.AspNetCore.Mvc.Localization;

/// <summary>
/// Provides extension members for <see cref="LocalizedHtmlString"/>.
/// </summary>
public static class LocalizedHtmlStringExtensions
{
    extension(LocalizedHtmlString)
    {
        /// <summary>
        /// Creates a <see cref="LocalizedHtmlString"/> that uses the same text for its name and its value.
        /// </summary>
        /// <param name="name">The name of the string. It is also used as the value.</param>
        /// <returns>A <see cref="LocalizedHtmlString"/> where <see cref="LocalizedHtmlString.Name"/> and <see cref="LocalizedHtmlString.Value"/> are equal to <paramref name="name"/>.</returns>
        /// <remarks>
        /// The returned string is not translated. The name can be used as the key to localize the string later.
        /// Like the <see cref="LocalizedHtmlString"/> constructor, the value is rendered as HTML and is not encoded.
        /// </remarks>
        public static LocalizedHtmlString Create(string name)
            => new(name, name);
    }
}
