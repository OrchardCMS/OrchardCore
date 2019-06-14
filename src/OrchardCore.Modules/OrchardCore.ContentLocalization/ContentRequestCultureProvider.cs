using System;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using YesSql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using OrchardCore.Autoroute.Services;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.ContentManagement;

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
            
            var path = httpContext.Request.PathBase + httpContext.Request.Path + httpContext.Request.QueryString;

            var culture = await culturePickerService.GetCultureFromRoute(path);
            if (!string.IsNullOrEmpty(culture))
            {
                return new ProviderCultureResult(culture);
            }
            return null;
        }
    }
}
