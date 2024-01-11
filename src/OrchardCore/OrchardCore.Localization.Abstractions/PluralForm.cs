using System.Globalization;

namespace OrchardCore.Localization
{
    /// <summary>
    /// Represents a pluralization form.
    /// </summary>
    public class PluralForm
    {
        /// <summary>
        /// Creates new instance of <see cref="PluralForm"/> with key and form.
        /// </summary>
        /// <param name="key">The localized string.</param>
        /// <param name="form">The plural form.</param>
        public PluralForm(string key, int form)
            : this(key, form, CultureInfo.CurrentUICulture)
        {

        }

        /// <summary>
        /// Creates new instance of <see cref="PluralForm"/> with key, form and culture.
        /// </summary>
        /// <param name="key">The localized string.</param>
        /// <param name="form">The plural form.</param>
        /// <param name="culture">The <see cref="CultureInfo"/>.</param>
        public PluralForm(string key, int form, CultureInfo culture)
        {
            Key = key;
            Form = form;
            Culture = culture;
        }

        /// <summary>
        /// The current UI culture.
        /// </summary>
        public CultureInfo Culture { get; }

        /// <summary>
        /// Get the plural form.
        /// </summary>
        public int Form { get; }

        /// <summary>
        /// Gets the localized string.
        /// </summary>
        public string Key { get; }
    }
}
