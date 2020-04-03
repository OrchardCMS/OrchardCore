using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Services;

namespace OrchardCore.ContentLocalization
{
    /// <summary>
    /// RequestCultureProvider that automatically sets the Culture of a request from the LocalizationPart.Culture property.
    /// </summary>
    public class ContentRequestCultureProvider : RequestCultureProvider
    {
        public override async Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            var culturePickerService = httpContext.RequestServices.GetService<IContentCulturePickerService>();
            var localization = await culturePickerService.GetLocalizationFromRouteAsync(httpContext.Request.Path);

            if (localization != null)
            {
                return new ProviderCultureResult(localization.Culture);
            }

            return null;
        }
    }
}
