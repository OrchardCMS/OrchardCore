using System.Globalization;

namespace OrchardCore.Localization.Models
{
    /// <summary>
    /// Represents an object to store the localization settings.
    /// </summary>
    public class LocalizationSettings
    {
        private static readonly string[] _defaultSupportedCultures = new[] { CultureInfo.InstalledUICulture.Name };

        /// <summary>
        /// Creates a new instance of the <see cref="LocalizationSettings"/>.
        /// </summary>
        public LocalizationSettings()
        {
            DefaultCulture = CultureInfo.InstalledUICulture.Name;
            SupportedCultures = _defaultSupportedCultures;
        }

        /// <summary>
        /// Gets or sets the default culture of the site.
        /// </summary>
        public string DefaultCulture { get; set; }

        /// <summary>
        /// Gets or sets all the supported cultures of the site. It also contains the default culture.
        /// </summary>
        public string[] SupportedCultures { get; set; }
    }
}
