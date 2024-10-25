using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization;

/// <summary>
/// RequestCultureProvider that automatically sets the Culture of a request from the LocalizationPart.Culture property.
/// </summary>
public class ContentRequestCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var culturePickerService = httpContext.RequestServices.GetService<IContentCulturePickerService>();
        var siteService = httpContext.RequestServices.GetService<ISiteService>();
        var localization = await culturePickerService.GetLocalizationFromRouteAsync(httpContext.Request.Path);

        if (localization != null)
        {
            var settings = await siteService.GetSettingsAsync<ContentRequestCultureProviderSettings>();
            if (settings.SetCookie)
            {
                culturePickerService.SetContentCulturePickerCookie(localization.Culture);
            }

            return new ProviderCultureResult(localization.Culture);
        }

        return default;
    }
}
