using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement;
using OrchardCore.Entities;
using OrchardCore.Media;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Seo.Models;
using OrchardCore.Settings;

namespace OrchardCore.Seo.Drivers;

public class SeoMetaSettingsHandler : ContentHandlerBase
{
    private readonly IMediaFileStore _mediaFileStore;
    private readonly ISiteService _siteService;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private IContentManager _contentManager;

    public SeoMetaSettingsHandler(
        IMediaFileStore mediaFileStore,
        ISiteService siteService,
        IActionContextAccessor actionContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        IUrlHelperFactory urlHelperFactory
        )
    {
        _mediaFileStore = mediaFileStore;
        _siteService = siteService;
        _actionContextAccessor = actionContextAccessor;
        _httpContextAccessor = httpContextAccessor;
        _urlHelperFactory = urlHelperFactory;
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
    {
        return context.ForAsync<SeoAspect>(async aspect =>
        {
            // This handlers provides defaults, either from the Seo Meta Settings, or ensures values by default. (title etc)
            _contentManager ??= _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IContentManager>();
            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var metaSettings = siteSettings.As<ContentItem>("SocialMetaSettings");

            var actionContext = _actionContextAccessor.ActionContext;

            actionContext ??= await GetActionContextAsync(_httpContextAccessor.HttpContext);

            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(context.ContentItem);

            var relativeUrl = urlHelper.RouteUrl(contentItemMetadata.DisplayRouteValues);
            var absoluteUrl = urlHelper.ToAbsoluteUrl(relativeUrl);

            // Logic is this happens last after the part settings.
            // so if values are not set it is responsible for settings them.

            string defaultImage = metaSettings.Content.SocialMetaSettings?.DefaultSocialImage?.Paths?.Count > 0 ? metaSettings.Content.SocialMetaSettings.DefaultSocialImage.Paths[0] : string.Empty;
            string openGraphImage = metaSettings.Content.SocialMetaSettings?.OpenGraphImage?.Paths?.Count > 0 ? metaSettings.Content.SocialMetaSettings?.OpenGraphImage?.Paths[0] : string.Empty;
            string twitterImage = metaSettings.Content.SocialMetaSettings?.TwitterImage?.Paths?.Count > 0 ? metaSettings.Content.SocialMetaSettings?.TwitterImage?.Paths[0] : string.Empty;

            string defaultAltText = metaSettings.Content.SocialMetaSettings?.DefaultSocialImage?.MediaTexts?.Count > 0 ? metaSettings.Content.SocialMetaSettings.DefaultSocialImage.MediaTexts[0] : string.Empty;
            string openGraphAltText = metaSettings.Content.SocialMetaSettings?.OpenGraphImage?.MediaTexts?.Count > 0 ? metaSettings.Content.SocialMetaSettings?.OpenGraphImage?.MediaTexts[0] : string.Empty;
            string twitterAltText = metaSettings.Content.SocialMetaSettings?.TwitterImage?.MediaTexts?.Count > 0 ? metaSettings.Content.SocialMetaSettings?.TwitterImage?.MediaTexts[0] : string.Empty;

            string twitterCard = metaSettings.Content.SocialMetaSettings?.TwitterCard?.Text?.ToString();
            string twitterCreator = metaSettings.Content.SocialMetaSettings?.TwitterCreator?.Text?.ToString();
            string twitterSite = metaSettings.Content.SocialMetaSettings?.TwitterSite?.Text?.ToString();

            string googleSchema = metaSettings.Content.SocialMetaSettings?.GoogleSchema?.Text?.ToString();

            // Meta

            if (string.IsNullOrEmpty(aspect.MetaDescription))
            {
                aspect.MetaDescription = metaSettings.Content.SocialMetaSettings?.DefaultMetaDescription?.Text?.ToString();
            }

            // OpenGraph

            aspect.OpenGraphUrl = aspect.Canonical ??= absoluteUrl;

            if (string.IsNullOrEmpty(aspect.OpenGraphType))
            {
                aspect.OpenGraphType = metaSettings.Content.SocialMetaSettings?.OpenGraphType?.Text?.ToString();
            }

            if (string.IsNullOrEmpty(aspect.OpenGraphTitle))
            {
                aspect.OpenGraphTitle = context.ContentItem.DisplayText;
            }

            if (string.IsNullOrEmpty(aspect.OpenGraphDescription))
            {
                aspect.OpenGraphDescription = metaSettings.Content.SocialMetaSettings?.DefaultOpenGraphDescription?.Text?.ToString();
            }

            if (string.IsNullOrEmpty(aspect.OpenGraphSiteName))
            {
                aspect.OpenGraphSiteName = metaSettings.Content.SocialMetaSettings?.OpenGraphSiteName?.Text?.ToString();
                if (string.IsNullOrEmpty(aspect.OpenGraphSiteName))
                {
                    aspect.OpenGraphSiteName = siteSettings.SiteName;
                }
            }

            if (string.IsNullOrEmpty(aspect.OpenGraphAppId))
            {
                aspect.OpenGraphAppId = metaSettings.Content.SocialMetaSettings?.OpenGraphAppId?.Text?.ToString();
            }

            if (string.IsNullOrEmpty(aspect.OpenGraphImage))
            {
                if (string.IsNullOrEmpty(openGraphImage))
                {
                    if (!string.IsNullOrEmpty(defaultImage))
                    {
                        aspect.OpenGraphImage = urlHelper.ToAbsoluteUrl(_mediaFileStore.MapPathToPublicUrl(defaultImage));
                    }
                }
                else
                {
                    aspect.OpenGraphImage = urlHelper.ToAbsoluteUrl(_mediaFileStore.MapPathToPublicUrl(openGraphImage));
                }
            }

            if (string.IsNullOrEmpty(aspect.OpenGraphImageAlt))
            {
                if (string.IsNullOrEmpty(openGraphAltText))
                {
                    aspect.OpenGraphImageAlt = defaultAltText;
                }
                else
                {
                    aspect.OpenGraphImageAlt = openGraphAltText;
                }
            }

            // Twitter
            aspect.TwitterUrl = aspect.Canonical ??= absoluteUrl;

            if (string.IsNullOrEmpty(aspect.TwitterTitle))
            {
                aspect.TwitterTitle = context.ContentItem.DisplayText;
            }

            if (string.IsNullOrEmpty(aspect.TwitterDescription))
            {
                aspect.TwitterDescription = metaSettings.Content.SocialMetaSettings?.DefaultTwitterDescription?.Text?.ToString();
            }

            if (string.IsNullOrEmpty(aspect.TwitterCard))
            {
                aspect.TwitterCard = twitterCard;
            }

            if (string.IsNullOrEmpty(aspect.TwitterSite))
            {
                aspect.TwitterSite = twitterSite;
            }

            if (string.IsNullOrEmpty(aspect.TwitterCreator))
            {
                aspect.TwitterCreator = twitterCreator;
            }

            if (string.IsNullOrEmpty(aspect.TwitterImage))
            {
                if (string.IsNullOrEmpty(twitterImage))
                {
                    if (!string.IsNullOrEmpty(defaultImage))
                    {
                        aspect.TwitterImage = urlHelper.ToAbsoluteUrl(_mediaFileStore.MapPathToPublicUrl(defaultImage));
                    }
                }
                else
                {
                    aspect.TwitterImage = urlHelper.ToAbsoluteUrl(_mediaFileStore.MapPathToPublicUrl(twitterImage));
                }
            }

            if (string.IsNullOrEmpty(aspect.TwitterImageAlt))
            {
                if (string.IsNullOrEmpty(twitterAltText))
                {
                    aspect.TwitterImageAlt = defaultAltText;
                }
                else
                {
                    aspect.TwitterImageAlt = twitterAltText;
                }
            }

            if (string.IsNullOrEmpty(aspect.GoogleSchema))
            {
                aspect.GoogleSchema = googleSchema;
            }
        });
    }

    internal static async Task<ActionContext> GetActionContextAsync(HttpContext httpContext)
    {
        var routeData = new RouteData();
        routeData.Routers.Add(new RouteCollection());

        var actionContext = new ActionContext(httpContext, routeData, new ActionDescriptor());
        var filters = httpContext.RequestServices.GetServices<IAsyncViewActionFilter>();

        foreach (var filter in filters)
        {
            await filter.OnActionExecutionAsync(actionContext);
        }

        return actionContext;
    }
}
