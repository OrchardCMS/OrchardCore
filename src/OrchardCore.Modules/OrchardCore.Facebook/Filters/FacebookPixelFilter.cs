using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Admin;
using OrchardCore.Facebook.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Filters;

public sealed class FacebookPixelFilter : IAsyncResultFilter
{
    private readonly IResourceManager _resourceManager;
    private readonly ISiteService _siteService;
    private readonly JavaScriptEncoder _jsEncoder;

    private static readonly HtmlString _preamble = new("<!-- Meta Pixel Code -->\r\n<script>\r\n!function(f,b,e,v,n,t,s)\r\n{if(f.fbq)return;n=f.fbq=function(){n.callMethod?\r\nn.callMethod.apply(n,arguments):n.queue.push(arguments)};\r\nif(!f._fbq)f._fbq=n;n.push=n;n.loaded=!0;n.version='2.0';\r\nn.queue=[];t=b.createElement(e);t.async=!0;\r\nt.src=v;s=b.getElementsByTagName(e)[0];\r\ns.parentNode.insertBefore(t,s)}(window, document,'script',\r\n'https://connect.facebook.net/en_US/fbevents.js');\r\nfbq('init', '");
    private static readonly HtmlString _end = new HtmlString("');\r\nfbq('track', 'PageView');\r\n</script>\r\n<!-- End Meta Pixel Code -->");

    public FacebookPixelFilter(
        IResourceManager resourceManager,
        ISiteService siteService,
        JavaScriptEncoder jsEncoder)
    {
        _resourceManager = resourceManager;
        _siteService = siteService;
        _jsEncoder = jsEncoder;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Should only run on the front-end for a full view.
        if (context.IsViewOrPageResult() && !AdminAttribute.IsApplied(context.HttpContext))
        {
            var canTrack = context.HttpContext.Features.Get<ITrackingConsentFeature>()?.CanTrack ?? true;

            if (canTrack)
            {
                var settings = await _siteService.GetSettingsAsync<FacebookPixelSettings>();

                if (!string.IsNullOrEmpty(settings?.PixelId))
                {
                    _resourceManager.RegisterHeadScript(new HtmlContentBuilder([_preamble, _jsEncoder.Encode(settings.PixelId), _end]));
                }
            }
        }

        await next.Invoke();
    }
}
