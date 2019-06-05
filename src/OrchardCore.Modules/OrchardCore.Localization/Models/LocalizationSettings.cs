using System.Globalization;

namespace OrchardCore.Localization.Models
{
    public class LocalizationSettings
    {
        private static string[] DefaultSupportedCultures = new[] { CultureInfo.InstalledUICulture.Name };

        public LocalizationSettings()
        {
            DefaultCulture = CultureInfo.InstalledUICulture.Name;
            SupportedCultures = DefaultSupportedCultures;
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
