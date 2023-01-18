using System;
using Microsoft.Extensions.DependencyInjection;
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

            app.UseMiddleware<CultureAliasConversionMiddleware>(cultureAliasProvider);

            return app;
        }
    }
}
