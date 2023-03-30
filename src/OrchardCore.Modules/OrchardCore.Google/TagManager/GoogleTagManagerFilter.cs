using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Google.TagManager.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Google.TagManager
{
    public class GoogleTagManagerFilter : IAsyncResultFilter
    {
        private readonly IResourceManager _resourceManager;
        private readonly ISiteService _siteService;

        private HtmlString _scriptsCache;

        public GoogleTagManagerFilter(
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
                    var settings = (await _siteService.GetSiteSettingsAsync()).As<GoogleTagManagerSettings>();

                    if (!string.IsNullOrWhiteSpace(settings?.ContainerID))
                    {
                        _scriptsCache = new HtmlString("<!-- Google Tag Manager -->\n<script>(function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({'gtm.start':\n  new Date().getTime(),event:'gtm.js'});var f=d.getElementsByTagName(s)[0],\n  j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';j.async=true;j.src=\n  'https://www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j,f);\n  })(window,document,'script','dataLayer','" + settings.ContainerID + "');</script>\n<!-- End Google Tag Manager -->");
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
