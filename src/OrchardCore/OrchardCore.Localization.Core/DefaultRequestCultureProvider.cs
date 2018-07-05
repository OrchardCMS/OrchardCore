using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Settings;

namespace OrchardCore.Localization
{
    public class DefaultRequestCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult> DetermineProviderCultureResult(HttpContext httpContext)
        {
            var siteService = httpContext.RequestServices.GetRequiredService<ISiteService>();
            var siteCulture = siteService.GetSiteSettingsAsync().Result?.Culture ?? CultureInfo.InvariantCulture.Name;
            return Task.FromResult(new ProviderCultureResult(siteCulture));
        }
    }
}
