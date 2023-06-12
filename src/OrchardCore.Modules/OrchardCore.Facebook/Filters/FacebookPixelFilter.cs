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
        // Should only run on the front-end for a full view
        if ((context.Result is ViewResult || context.Result is PageResult) &&
            !AdminAttribute.IsApplied(context.HttpContext))
        {
            var canTrack = context.HttpContext.Features.Get<ITrackingConsentFeature>()?.CanTrack ?? true;

            if (_scriptsCache == null && canTrack)
            {
                var settings = (await _siteService.GetSiteSettingsAsync()).As<FacebookPixelSettings>();

                if (!String.IsNullOrWhiteSpace(settings?.PixelId))
                {
                    _scriptsCache = new HtmlString($"<!-- Meta Pixel Code --><script>\n!function(f,b,e,v,n,t,s)\n{{if(f.fbq)return;n=f.fbq=function(){{n.callMethod?\r\n  n.callMethod.apply(n,arguments):n.queue.push(arguments)}};\n if(!f._fbq)\n f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';\n  n.queue=[];t=b.createElement(e);t.async=!0;\r\n  t.src=v;s=b.getElementsByTagName(e)[0];\n  s.parentNode.insertBefore(t,s)}}(window, document,'script',\r\n  'https://connect.facebook.net/en_US/fbevents.js');\n  fbq('init', '{settings?.PixelId}');\n  fbq('track', 'PageView');\n</script> \n<noscript><img height=\"1\" width=\"1\" style=\"display:none\"\r\n  src=\"https://www.facebook.com/tr?id={settings?.PixelId}&ev=PageView&noscript=1\"\r\n/></noscript> \n <!-- End Meta Pixel Code -->");
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
