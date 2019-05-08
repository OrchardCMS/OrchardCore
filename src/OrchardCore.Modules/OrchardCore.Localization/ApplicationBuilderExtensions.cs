using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;

namespace OrchardCore.Localization
{
    public static class ApplicationBuilderExtensions
    {
        public static void UsePortableObjectLocalization(this IApplicationBuilder app, RequestLocalizationOptions localizationOptions, string defaultCulture, string[] supportedCultures)
        {
            // If no specific default culture is defined, use the system language by not calling SetDefaultCulture
            if (!String.IsNullOrEmpty(defaultCulture))
            {
                localizationOptions.SetDefaultCulture(defaultCulture);
            }

            if (supportedCultures?.Length > 0)
            {
                supportedCultures
                    .Concat(new[] {localizationOptions.DefaultRequestCulture.Culture.Name})
                    .Distinct()
                    .ToArray();

                localizationOptions
                    .AddSupportedCultures(supportedCultures)
                    .AddSupportedUICultures(supportedCultures)
                    ;
            }

            app.UseRequestLocalization(localizationOptions);
        }
    }
}