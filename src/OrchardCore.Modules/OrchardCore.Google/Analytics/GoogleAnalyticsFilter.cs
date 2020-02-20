using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Google.Analytics.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Google.Analytics
{
    public class GoogleAnalyticsFilter : IAsyncResultFilter
    {
        private IResourceManager _resourceManager;
        private ISiteService _siteService;

        private HtmlString _scriptsCache;


        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // Should only run on the front-end for a full view
            if ((context.Result is ViewResult || context.Result is PageResult) &&
                !AdminAttribute.IsApplied(context.HttpContext))
            {
                if (_scriptsCache == null)
                {
                    // Resolve scoped services lazy if we got this far.
                    _siteService ??= context.HttpContext.RequestServices.GetRequiredService<ISiteService>();

                    var settings = (await _siteService.GetSiteSettingsAsync()).As<GoogleAnalyticsSettings>();

                    if (!string.IsNullOrWhiteSpace(settings?.TrackingID))
                    {
                        _scriptsCache = new HtmlString($"<script async src=\"https://www.googletagmanager.com/gtag/js?id={settings.TrackingID}\"></script>\n<script>window.dataLayer = window.dataLayer || [];function gtag() {{ dataLayer.push(arguments); }}gtag('js', new Date());gtag('config', '{settings.TrackingID}')</script>");
                    }
                }

                if (_scriptsCache != null)
                {
                    // Resolve scoped services lazy if we got this far.
                    _resourceManager ??= context.HttpContext.RequestServices.GetRequiredService<IResourceManager>();

                    _resourceManager.RegisterHeadScript(_scriptsCache);
                }
            }

            await next.Invoke();
        }
    }
}
