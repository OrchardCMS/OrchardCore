namespace Microsoft.AspNetCore.Mvc.Localization;

/// <summary>
/// Provides extension members for <see cref="LocalizedHtmlString"/>.
/// </summary>
public static class LocalizedHtmlStringExtensions
{
    extension(LocalizedHtmlString)
    {
        /// <summary>
        /// Creates a <see cref="LocalizedHtmlString"/> from a value. The value is also used as the name.
        /// </summary>
        /// <param name="value">The value of the string. It is also used as the name.</param>
        /// <param name="arguments">The values to format the <paramref name="value"/> with when the string is rendered.</param>
        /// <returns>A <see cref="LocalizedHtmlString"/> where <see cref="LocalizedHtmlString.Name"/> and <see cref="LocalizedHtmlString.Value"/> are equal to <paramref name="value"/>.</returns>
        /// <remarks>
        /// The returned string is not translated. The name can be used as the key to localize the string later.
        /// Like the <see cref="LocalizedHtmlString"/> constructor, the value is rendered as HTML and is not encoded,
        /// and the <paramref name="arguments"/> are HTML encoded when the string is rendered.
        /// </remarks>
        public static LocalizedHtmlString Create(string value, params object[] arguments)
        {
            ArgumentNullException.ThrowIfNull(value);

            return new(value, value, isResourceNotFound: false, arguments ?? []);
        }
    }
}
