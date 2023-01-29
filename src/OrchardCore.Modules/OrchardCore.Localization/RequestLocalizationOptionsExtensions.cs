using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;

namespace OrchardCore.Localization
{
    internal static class RequestLocalizationOptionsExtensions
    {
        public static RequestLocalizationOptions SetDefaultCulture(
            this RequestLocalizationOptions options,
            bool useUserOverride,
            string defaultCulture)
        {
            options.DefaultRequestCulture = new RequestCulture(new CultureInfo(defaultCulture, useUserOverride));

            return options;
        }

        public static RequestLocalizationOptions AddSupportedCultures(
            this RequestLocalizationOptions options,
            bool useUserOverride,
            params string[] cultures)
        {
            var supportedCultures = new List<CultureInfo>(cultures.Length);

            foreach (var culture in cultures.Distinct())
            {
                supportedCultures.Add(new CultureInfo(culture, useUserOverride));
            }

            options.SupportedCultures = supportedCultures;

            return options;
        }

        public static RequestLocalizationOptions AddSupportedUICultures(
            this RequestLocalizationOptions options,
            bool useUserOverride,
            params string[] uiCultures)
        {
            var supportedUICultures = new List<CultureInfo>(uiCultures.Length);

            foreach (var culture in uiCultures.Distinct())
            {
                supportedUICultures.Add(new CultureInfo(culture, useUserOverride));
            }

            options.SupportedUICultures = supportedUICultures;

            return options;
        }
    }
}
