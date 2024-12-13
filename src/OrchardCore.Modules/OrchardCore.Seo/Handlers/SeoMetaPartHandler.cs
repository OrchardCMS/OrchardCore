using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.Extensions;
using OrchardCore.Media;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Seo.Models;
using OrchardCore.Settings;

namespace OrchardCore.Seo.Drivers;

public class SeoMetaPartHandler : ContentPartHandler<SeoMetaPart>
{
    private readonly IMediaFileStore _mediaFileStore;
    private readonly ISiteService _siteService;
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IContentManager _contentManager;

    public SeoMetaPartHandler(
        IMediaFileStore mediaFileStore,
        ISiteService siteService,
        IActionContextAccessor actionContextAccessor,
        IHttpContextAccessor httpContextAccessor,
        IUrlHelperFactory urlHelperFactory,
        IContentManager contentManager
        )
    {
        _mediaFileStore = mediaFileStore;
        _siteService = siteService;
        _actionContextAccessor = actionContextAccessor;
        _httpContextAccessor = httpContextAccessor;
        _urlHelperFactory = urlHelperFactory;
        _contentManager = contentManager;
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context, SeoMetaPart part)
    {
        return context.ForAsync<SeoAspect>(async aspect =>
        {
            aspect.Render = part.Render;

            if (!string.IsNullOrEmpty(part.PageTitle))
            {
                aspect.PageTitle = part.PageTitle;
            }

            if (!string.IsNullOrEmpty(part.MetaDescription))
            {
                aspect.MetaDescription = part.MetaDescription;
            }

            if (!string.IsNullOrEmpty(part.MetaKeywords))
            {
                aspect.MetaKeywords = part.MetaKeywords;
            }

            if (!string.IsNullOrEmpty(part.MetaRobots))
            {
                aspect.MetaRobots = part.MetaRobots;
            }

            aspect.CustomMetaTags = part.CustomMetaTags;

            var siteSettings = await _siteService.GetSiteSettingsAsync();

            var actionContext = _actionContextAccessor.ActionContext;

            actionContext ??= await _httpContextAccessor.GetActionContextAsync();

            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

            if (!string.IsNullOrEmpty(part.Canonical))
            {
                aspect.Canonical = part.Canonical;
            }
            else
            {
                var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(part.ContentItem);
                var relativeUrl = urlHelper.RouteUrl(contentItemMetadata.DisplayRouteValues);
                aspect.Canonical = urlHelper.ToAbsoluteUrl(relativeUrl);
            }

            // OpenGraph
            if (part.OpenGraphImage?.Paths?.Length > 0)
            {
                aspect.OpenGraphImage = urlHelper.ToAbsoluteUrl(_mediaFileStore.MapPathToPublicUrl(part.OpenGraphImage.Paths[0]));
            }
            else if (part.DefaultSocialImage?.Paths?.Length > 0)
            {
                aspect.OpenGraphImage = urlHelper.ToAbsoluteUrl(_mediaFileStore.MapPathToPublicUrl(part.DefaultSocialImage.Paths[0]));
            }

            if (part.OpenGraphImage?.MediaTexts?.Length > 0)
            {
                aspect.OpenGraphImageAlt = part.OpenGraphImage.MediaTexts[0];
            }
            else if (part.DefaultSocialImage?.MediaTexts?.Length > 0)
            {
                aspect.OpenGraphImageAlt = part.DefaultSocialImage.MediaTexts[0];
            }

            if (!string.IsNullOrEmpty(part.OpenGraphTitle))
            {
                aspect.OpenGraphTitle = part.OpenGraphTitle;
            }
            else
            {
                aspect.OpenGraphTitle = part.PageTitle;
            }

            if (!string.IsNullOrEmpty(part.OpenGraphDescription))
            {
                aspect.OpenGraphDescription = part.OpenGraphDescription;
            }
            else
            {
                aspect.OpenGraphDescription = part.MetaDescription;
            }

            // Twitter

            if (part.TwitterImage?.Paths?.Length > 0)
            {
                aspect.TwitterImage = urlHelper.ToAbsoluteUrl(_mediaFileStore.MapPathToPublicUrl(part.TwitterImage.Paths[0]));
            }
            else if (part.DefaultSocialImage?.Paths?.Length > 0)
            {
                aspect.TwitterImage = urlHelper.ToAbsoluteUrl(_mediaFileStore.MapPathToPublicUrl(part.DefaultSocialImage.Paths[0]));
            }

            if (part.TwitterImage?.MediaTexts?.Length > 0)
            {
                aspect.TwitterImageAlt = part.TwitterImage.MediaTexts[0];
            }
            else if (part.DefaultSocialImage?.MediaTexts?.Length > 0)
            {
                aspect.TwitterImageAlt = part.DefaultSocialImage.MediaTexts[0];
            }

            if (!string.IsNullOrEmpty(part.TwitterTitle))
            {
                aspect.TwitterTitle = part.TwitterTitle;
            }
            else
            {
                aspect.TwitterTitle = part.PageTitle;
            }

            if (!string.IsNullOrEmpty(part.TwitterDescription))
            {
                aspect.TwitterDescription = part.TwitterDescription;
            }
            else
            {
                aspect.TwitterDescription = part.MetaDescription;
            }

            if (!string.IsNullOrEmpty(part.TwitterCard))
            {
                aspect.TwitterCard = part.TwitterCard;
            }

            if (!string.IsNullOrEmpty(part.TwitterCreator))
            {
                aspect.TwitterCreator = part.TwitterCreator;
            }

            if (!string.IsNullOrEmpty(part.TwitterSite))
            {
                aspect.TwitterSite = part.TwitterSite;
            }

            aspect.GoogleSchema = part.GoogleSchema;
        });
    }
}
