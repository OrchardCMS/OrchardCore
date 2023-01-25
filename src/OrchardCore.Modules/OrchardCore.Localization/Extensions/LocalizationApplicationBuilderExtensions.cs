using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Localization;
using OrchardCore.Localization.Services;

namespace Microsoft.AspNetCore.Builder
{
    public static class LocalizationApplicationBuilderExtensions
    {
        public static IApplicationBuilder MapCulturesAlias(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app, nameof(app));

            var cultureAliasProvider = app.ApplicationServices.GetService<ICultureAliasProvider>();
            var requestLocalizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();

            app.UseMiddleware<CultureAliasConversionMiddleware>(cultureAliasProvider, requestLocalizationOptions);

            return app;
        }
    }
}
