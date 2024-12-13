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

    private HtmlString _scriptsCache;

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
        if (context.IsViewOrPageResult() && !AdminAttribute.IsApplied(context.HttpContext))
        {
            var canTrack = context.HttpContext.Features.Get<ITrackingConsentFeature>()?.CanTrack ?? true;

            if (_scriptsCache == null && canTrack)
            {
                var settings = await _siteService.GetSettingsAsync<GoogleAnalyticsSettings>();

                if (!string.IsNullOrWhiteSpace(settings?.TrackingID))
                {
                    _scriptsCache = new HtmlString($"<!-- Global site tag (gtag.js) - Google Analytics -->\n<script async src=\"https://www.googletagmanager.com/gtag/js?id={settings.TrackingID}\"></script>\n<script>window.dataLayer = window.dataLayer || [];function gtag() {{ dataLayer.push(arguments); }}gtag('js', new Date());gtag('config', '{settings.TrackingID}')</script>\n<!-- End Global site tag (gtag.js) - Google Analytics -->");
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
