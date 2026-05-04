using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.DisplayManagement.Extensions;
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
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private IContentManager _contentManager;

    public SeoMetaSettingsHandler(
        IMediaFileStore mediaFileStore,
        ISiteService siteService,
        IHttpContextAccessor httpContextAccessor,
        IUrlHelperFactory urlHelperFactory
        )
    {
        _mediaFileStore = mediaFileStore;
        _siteService = siteService;
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

            // In .NET 10, create ActionContext directly instead of using obsolete IActionContextAccessor
            var httpContext = _httpContextAccessor.HttpContext;
            var actionContext = await httpContext.GetActionContextAsync();

            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(context.ContentItem);

            var relativeUrl = urlHelper.RouteUrl(contentItemMetadata.DisplayRouteValues);
            var absoluteUrl = urlHelper.ToAbsoluteUrl(relativeUrl);

            // Logic is this happens last after the part settings.
            // so if values are not set it is responsible for settings them.

            var defaultImage = string.Empty;
            var openGraphImage = string.Empty;
            var twitterImage = string.Empty;

            var defaultAltText = string.Empty;
            var openGraphAltText = string.Empty;
            var twitterAltText = string.Empty;

            var defaultMetaDescription = string.Empty;
            var openGraphType = string.Empty;
            var defaultOpenGraphDescription = string.Empty;
            var openGraphSiteName = string.Empty;
            var openGraphAppId = string.Empty;
            var defaultTwitterDescription = string.Empty;
            var twitterCard = string.Empty;
            var twitterCreator = string.Empty;
            var twitterSite = string.Empty;
            var googleSchema = string.Empty;

            if (siteSettings.TryGet<ContentItem>("SocialMetaSettings", out var metaSettings))
            {
                dynamic socialMetaSettings = metaSettings.Content?.SocialMetaSettings;

                if (socialMetaSettings is not null)
                {
                    if (socialMetaSettings.DefaultSocialImage?.Paths?.Count > 0)
                    {
                        defaultImage = socialMetaSettings.DefaultSocialImage.Paths[0];
                    }

                    if (socialMetaSettings.OpenGraphImage?.Paths?.Count > 0)
                    {
                        openGraphImage = socialMetaSettings.OpenGraphImage.Paths[0];
                    }

                    if (socialMetaSettings.TwitterImage?.Paths?.Count > 0)
                    {
                        twitterImage = socialMetaSettings.TwitterImage.Paths[0];
                    }

                    if (socialMetaSettings.DefaultSocialImage?.MediaTexts?.Count > 0)
                    {
                        defaultAltText = socialMetaSettings.DefaultSocialImage.MediaTexts[0];
                    }

                    if (socialMetaSettings.OpenGraphImage?.MediaTexts?.Count > 0)
                    {
                        openGraphAltText = socialMetaSettings.OpenGraphImage.MediaTexts[0];
                    }

                    if (socialMetaSettings.TwitterImage?.MediaTexts?.Count > 0)
                    {
                        twitterAltText = socialMetaSettings.TwitterImage.MediaTexts[0];
                    }

                    if (socialMetaSettings.DefaultMetaDescription?.Text is not null)
                    {
                        defaultMetaDescription = socialMetaSettings.DefaultMetaDescription.Text.ToString();
                    }

                    if (socialMetaSettings.OpenGraphType?.Text is not null)
                    {
                        openGraphType = socialMetaSettings.OpenGraphType.Text.ToString();
                    }

                    if (socialMetaSettings.DefaultOpenGraphDescription?.Text is not null)
                    {
                        defaultOpenGraphDescription = socialMetaSettings.DefaultOpenGraphDescription.Text.ToString();
                    }

                    if (socialMetaSettings.OpenGraphSiteName?.Text is not null)
                    {
                        openGraphSiteName = socialMetaSettings.OpenGraphSiteName.Text.ToString();
                    }

                    if (socialMetaSettings.OpenGraphAppId?.Text is not null)
                    {
                        openGraphAppId = socialMetaSettings.OpenGraphAppId.Text.ToString();
                    }

                    if (socialMetaSettings.DefaultTwitterDescription?.Text is not null)
                    {
                        defaultTwitterDescription = socialMetaSettings.DefaultTwitterDescription.Text.ToString();
                    }

                    if (socialMetaSettings.TwitterCard?.Text is not null)
                    {
                        twitterCard = socialMetaSettings.TwitterCard.Text.ToString();
                    }

                    if (socialMetaSettings.TwitterCreator?.Text is not null)
                    {
                        twitterCreator = socialMetaSettings.TwitterCreator.Text.ToString();
                    }

                    if (socialMetaSettings.TwitterSite?.Text is not null)
                    {
                        twitterSite = socialMetaSettings.TwitterSite.Text.ToString();
                    }

                    if (socialMetaSettings.GoogleSchema?.Text is not null)
                    {
                        googleSchema = socialMetaSettings.GoogleSchema.Text.ToString();
                    }
                }
            }

            // Meta
            if (string.IsNullOrEmpty(aspect.MetaDescription))
            {
                aspect.MetaDescription = defaultMetaDescription;
            }

            // OpenGraph
            aspect.OpenGraphUrl = aspect.Canonical ??= absoluteUrl;

            if (string.IsNullOrEmpty(aspect.OpenGraphType))
            {
                aspect.OpenGraphType = openGraphType;
            }

            if (string.IsNullOrEmpty(aspect.OpenGraphTitle))
            {
                aspect.OpenGraphTitle = context.ContentItem.DisplayText;
            }

            if (string.IsNullOrEmpty(aspect.OpenGraphDescription))
            {
                aspect.OpenGraphDescription = defaultOpenGraphDescription;
            }

            if (string.IsNullOrEmpty(aspect.OpenGraphSiteName))
            {
                aspect.OpenGraphSiteName = openGraphSiteName;
                if (string.IsNullOrEmpty(aspect.OpenGraphSiteName))
                {
                    aspect.OpenGraphSiteName = siteSettings.SiteName;
                }
            }

            if (string.IsNullOrEmpty(aspect.OpenGraphAppId))
            {
                aspect.OpenGraphAppId = openGraphAppId;
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
                aspect.TwitterDescription = defaultTwitterDescription;
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
}
