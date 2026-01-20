using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Admin;
using OrchardCore.Google.TagManager.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Google.TagManager;

public sealed class GoogleTagManagerFilter : IAsyncResultFilter
{
    private readonly IResourceManager _resourceManager;
    private readonly ISiteService _siteService;
    private readonly JavaScriptEncoder _jsEncoder;

    private static readonly HtmlString _preamble = new("<!-- Google Tag Manager -->\n<script>(function(w,d,s,l,i){w[l]=w[l]||[];w[l].push({'gtm.start':\n  new Date().getTime(),event:'gtm.js'});var f=d.getElementsByTagName(s)[0],\n  j=d.createElement(s),dl=l!='dataLayer'?'&l='+l:'';j.async=true;j.src=\n  'https://www.googletagmanager.com/gtm.js?id='+i+dl;f.parentNode.insertBefore(j,f);\n  })(window,document,'script','dataLayer','");
    private static readonly HtmlString _end = new HtmlString("');</script>\n<!-- End Google Tag Manager -->");

    public GoogleTagManagerFilter(
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
        // Should only run on the front-end for a full view
        if (context.IsViewOrPageResult() && !AdminAttribute.IsApplied(context.HttpContext))
        {
            var canTrack = context.HttpContext.Features.Get<ITrackingConsentFeature>()?.CanTrack ?? true;

            if (canTrack)
            {
                var settings = await _siteService.GetSettingsAsync<GoogleTagManagerSettings>();

                if (!string.IsNullOrEmpty(settings?.ContainerID))
                {
                    _resourceManager.RegisterHeadScript(new HtmlContentBuilder([_preamble, _jsEncoder.Encode(settings.ContainerID), _end]));
                }
            }
        }

        await next.Invoke();
    }
}
