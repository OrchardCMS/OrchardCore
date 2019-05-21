using System;
using System.Globalization;
using System.Linq;

namespace OrchardCore.Settings
{
    public static class SiteExtensions
    {
        public static string[] GetConfiguredCultures(this ISite site)
        {
            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            var configuredCultures = new[] { site.Culture ?? CultureInfo.InstalledUICulture.Name };

            if (site.SupportedCultures != null)
            {
                configuredCultures = configuredCultures.Union(site.SupportedCultures).ToArray();
            }

            return configuredCultures;
        }
    }
}
