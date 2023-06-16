using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.Facebook.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Filters;

public class FacebookPixelFilter : IAsyncResultFilter
{
    private readonly IResourceManager _resourceManager;
    private readonly ISiteService _siteService;
    private readonly HtmlString _code = new("<!-- Meta Pixel Code -->\r\n<script>\r\n!function(f,b,e,v,n,t,s)\r\n{if(f.fbq)return;n=f.fbq=function(){n.callMethod?\r\nn.callMethod.apply(n,arguments):n.queue.push(arguments)};\r\nif(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';\r\nn.queue=[];t=b.createElement(e);t.async=!0;\r\nt.src=v;s=b.getElementsByTagName(e)[0];\r\ns.parentNode.insertBefore(t,s)}(window, document,'script',\r\n'https://connect.facebook.net/en_US/fbevents.js');\r\nfbq('init', MetaPixelId);\r\nfbq('track', 'PageView');\r\n</script>\r\n<!-- End Meta Pixel Code -->");

    private HtmlString _scriptsCache;

    public FacebookPixelFilter(
        IResourceManager resourceManager,
        ISiteService siteService)
    {
        _resourceManager = resourceManager;
        _siteService = siteService;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Should only run on the front-end for a full view.
        if ((context.Result is ViewResult || context.Result is PageResult)
            && !AdminAttribute.IsApplied(context.HttpContext))
        {
            var canTrack = context.HttpContext.Features.Get<ITrackingConsentFeature>()?.CanTrack ?? true;

            if (_scriptsCache == null && canTrack)
            {
                var settings = (await _siteService.GetSiteSettingsAsync()).As<FacebookPixelSettings>();

                if (!String.IsNullOrWhiteSpace(settings?.PixelId))
                {
                    _scriptsCache = new HtmlString($"<script>const MetaPixelId = '{settings.PixelId.Replace("'", "")}';</script>");
                }
            }

            if (_scriptsCache != null)
            {
                _resourceManager.RegisterHeadScript(_scriptsCache);
                _resourceManager.RegisterHeadScript(_code);
            }
        }

        await next.Invoke();
    }
}
