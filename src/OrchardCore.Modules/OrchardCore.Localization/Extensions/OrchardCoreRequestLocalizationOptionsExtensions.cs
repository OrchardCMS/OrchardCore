using System;
using Microsoft.AspNetCore.Builder;

namespace OrchardCore.Localization;

internal static class OrchardCoreRequestLocalizationOptionsExtensions
{
    public static OrchardCoreRequestLocalizationOptions WithRequestLocalizationOptions(
        this OrchardCoreRequestLocalizationOptions orchardCoreRequestLocalization, RequestLocalizationOptions requestLocalizationOptions)
    {
        if (orchardCoreRequestLocalization is null)
        {
            throw new ArgumentNullException(nameof(orchardCoreRequestLocalization));
        }

        if (requestLocalizationOptions is null)
        {
            throw new ArgumentNullException(nameof(requestLocalizationOptions));
        }

        // Avoid ignoring values for the current registered RequestLocalizationOptions
        orchardCoreRequestLocalization.ApplyCurrentCultureToResponseHeaders = requestLocalizationOptions.ApplyCurrentCultureToResponseHeaders;
        orchardCoreRequestLocalization.RequestCultureProviders = requestLocalizationOptions.RequestCultureProviders;
        orchardCoreRequestLocalization.FallBackToParentCultures = requestLocalizationOptions.FallBackToParentCultures;
        orchardCoreRequestLocalization.FallBackToParentUICultures = requestLocalizationOptions.FallBackToParentUICultures;

        return orchardCoreRequestLocalization;
    }
}
