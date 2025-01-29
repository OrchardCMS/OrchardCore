using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.Admin;
using OrchardCore.Google.Analytics.Settings;
using OrchardCore.ResourceManagement;
using OrchardCore.Settings;

namespace OrchardCore.Google.Analytics;

public sealed class GoogleAnalyticsFilter : IAsyncResultFilter
{
    private readonly IResourceManager _resourceManager;
    private readonly ISiteService _siteService;
    private readonly JavaScriptEncoder _jsEncoder;
    private readonly UrlEncoder _urlEncoder;

    private static readonly HtmlString _preamble = new($"<!-- Global site tag (gtag.js) - Google Analytics -->\n<script async src=\"https://www.googletagmanager.com/gtag/js?id=");
    private static readonly HtmlString _middle = new HtmlString($"\"></script>\n<script>window.dataLayer = window.dataLayer || [];function gtag() {{ dataLayer.push(arguments); }}gtag('js', new Date());gtag('config', '");
    private static readonly HtmlString _end = new HtmlString($"')</script>\n<!-- End Global site tag (gtag.js) - Google Analytics -->");

    public GoogleAnalyticsFilter(
        IResourceManager resourceManager,
        ISiteService siteService,
        JavaScriptEncoder jsEncoder,
        UrlEncoder urlEncoder)
    {
        _resourceManager = resourceManager;
        _siteService = siteService;
        _jsEncoder = jsEncoder;
        _urlEncoder = urlEncoder;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Should only run on the front-end for a full view
        if (context.IsViewOrPageResult() && !AdminAttribute.IsApplied(context.HttpContext))
        {
            var canTrack = context.HttpContext.Features.Get<ITrackingConsentFeature>()?.CanTrack ?? true;

            if (canTrack)
            {
                var settings = await _siteService.GetSettingsAsync<GoogleAnalyticsSettings>();

                if (!string.IsNullOrEmpty(settings?.TrackingID))
                {
                    _resourceManager.RegisterHeadScript(new HtmlContentBuilder([_preamble, _urlEncoder.Encode(settings.TrackingID), _middle, _jsEncoder.Encode(settings.TrackingID), _end]));
                }
            }
        }

        await next.Invoke();
    }
}
