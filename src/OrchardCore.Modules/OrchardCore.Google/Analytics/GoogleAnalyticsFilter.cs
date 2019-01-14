using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Google.Analytics.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Google.Analytics
{
    public class GoogleAnalyticsFilter : IAsyncResultFilter
    {
        private readonly IResourceManager _resourceManager;
        private readonly ISiteService _siteService;

        public GoogleAnalyticsFilter(
            IResourceManager resourceManager,
            ISiteService siteService)
        {
            _resourceManager = resourceManager;
            _siteService = siteService;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Should only run on the front-end for a full view
            if ((context.Result is ViewResult || context.Result is PageResult) &&
                !AdminAttribute.IsApplied(context.HttpContext))
            {
                var settings = (await _siteService.GetSiteSettingsAsync()).As<GoogleAnalyticsSettings>();
                if (!string.IsNullOrWhiteSpace(settings?.TrackingID))
                {
                    _resourceManager.RegisterHeadScript(new HtmlString($"<script async src=\"https://www.googletagmanager.com/gtag/js?id={settings.TrackingID}\"></script>"));
                    _resourceManager.RegisterHeadScript(new HtmlString($"<script>window.dataLayer = window.dataLayer || [];function gtag() {{ dataLayer.push(arguments); }}gtag('js', new Date());gtag('config', '{settings.TrackingID}')</script>"));
                }
            }
            await next.Invoke();
        }
    }


}
