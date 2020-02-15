using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Google.AdSense.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Google.AdSense
{
    public class GoogleAdSenseFilter : IAsyncResultFilter
    {
        private readonly IResourceManager _resourceManager;
        private readonly ISiteService _siteService;

        private HtmlString _scriptsCache;

        public GoogleAdSenseFilter(
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
                if (_scriptsCache == null)
                {
                    var settings = (await _siteService.GetSiteSettingsAsync()).As<GoogleAdSenseSettings>();

                    if (!string.IsNullOrWhiteSpace(settings?.PublisherID))
                    {
                        _scriptsCache = new HtmlString($"\n<script data-ad-client=\"{settings.PublisherID}\" async src=\"https://pagead2.googlesyndication.com/pagead/js/adsbygoogle.js\"></script>");
                    }
                }

                if (_scriptsCache != null)
                {
                    _resourceManager.RegisterHeadScript(_scriptsCache);
                }
            }

            await next.Invoke();
        }
    }
}