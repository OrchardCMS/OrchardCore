using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ResourceManagement;
using OrchardCore.Seo.Models;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Seo.Drivers;

public sealed class SeoContentDriver : ContentDisplayDriver
{
    private readonly IContentManager _contentManager;
    private readonly IPageTitleBuilder _pageTitleBuilder;
    private readonly IResourceManager _resourceManager;
    private readonly IShortcodeService _shortcodeService;
    private readonly HtmlEncoder _htmlEncoder;

    private bool _primaryContentRendered { get; set; }

    public SeoContentDriver(
        IContentManager contentManager,
        IPageTitleBuilder pageTitleBuilder,
        IResourceManager resourceManager,
        IShortcodeService shortcodeService,
        HtmlEncoder htmlEncoder
        )
    {
        _contentManager = contentManager;
        _pageTitleBuilder = pageTitleBuilder;
        _resourceManager = resourceManager;
        _shortcodeService = shortcodeService;
        _htmlEncoder = htmlEncoder;
    }

    public override async Task<IDisplayResult> DisplayAsync(ContentItem contentItem, BuildDisplayContext context)
    {
        // We only apply this on the primary content item, which is considered the first call to BuildDisplay.
        if (_primaryContentRendered)
        {
            return null;
        }

        _primaryContentRendered = true;

        // Do not include Widgets or any display type other than detail.
        if (context.DisplayType != "Detail" || context.Shape.TryGetProperty(nameof(ContentTypeSettings.Stereotype), out string _))
        {
            return null;
        }

        var aspect = await _contentManager.PopulateAspectAsync<SeoAspect>(contentItem);

        if (!aspect.Render)
        {
            return null;
        }

        var shortCodeContext = new Context
        {
            ["ContentItem"] = contentItem
        };

        if (!string.IsNullOrEmpty(aspect.PageTitle))
        {
            _pageTitleBuilder.SetFixedTitle(new HtmlString(_htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.PageTitle, shortCodeContext))));
        }

        if (!string.IsNullOrEmpty(aspect.MetaDescription))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Name = "description",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.MetaDescription, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.MetaKeywords))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Name = "keywords",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.MetaKeywords, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.Canonical))
        {
            _resourceManager.RegisterLink(new LinkEntry
            {
                Href = aspect.Canonical,
                Rel = "canonical"
            });
        }

        if (!string.IsNullOrEmpty(aspect.MetaRobots))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Name = "robots",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.MetaRobots, shortCodeContext))
            });
        }

        foreach (var customMetaTag in aspect.CustomMetaTags)
        {
            // Generate a new meta entry as the builder is prepopulated.
            _resourceManager.RegisterMeta(new MetaEntry(
                _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(customMetaTag.Name, shortCodeContext)),
                _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(customMetaTag.Property, shortCodeContext)),
                _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(customMetaTag.Content, shortCodeContext)),
                _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(customMetaTag.HttpEquiv, shortCodeContext)),
                _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(customMetaTag.Charset, shortCodeContext))));
        }

        // OpenGraph.
        if (!string.IsNullOrEmpty(aspect.OpenGraphType))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "og:type",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.OpenGraphType, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.OpenGraphTitle))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "og:title",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.OpenGraphTitle, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.OpenGraphDescription))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "og:description",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.OpenGraphDescription, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.OpenGraphImage))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "og:image",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.OpenGraphImage, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.OpenGraphImageAlt))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "og:image:alt",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.OpenGraphImageAlt, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.OpenGraphUrl))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "og:url",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.OpenGraphUrl, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.OpenGraphSiteName))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "og:site_name",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.OpenGraphSiteName, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.OpenGraphAppId))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "fb:app_id",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.OpenGraphAppId, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.OpenGraphLocale))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "og:locale",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.OpenGraphLocale, shortCodeContext))
            });
        }

        // Twitter.
        if (!string.IsNullOrEmpty(aspect.TwitterCard))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "twitter:card",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.TwitterCard, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.TwitterSite))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Property = "twitter:site",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.TwitterSite, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.TwitterTitle))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Name = "twitter:title",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.TwitterTitle, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.TwitterDescription))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Name = "twitter:description",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.TwitterDescription, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.TwitterImage))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Name = "twitter:image",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.TwitterImage, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.TwitterImageAlt))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Name = "twitter:image:alt",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.TwitterImageAlt, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.TwitterCreator))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Name = "twitter:creator",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.TwitterCreator, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.TwitterUrl))
        {
            _resourceManager.RegisterMeta(new MetaEntry
            {
                Name = "twitter:url",
                Content = _htmlEncoder.Encode(await _shortcodeService.ProcessAsync(aspect.TwitterUrl, shortCodeContext))
            });
        }

        if (!string.IsNullOrEmpty(aspect.GoogleSchema))
        {
            var json = await _shortcodeService.ProcessAsync(aspect.GoogleSchema, shortCodeContext);

            try
            {
                // Validate json format
                JsonDocument.Parse(json);
            }
            catch
            {
                json = "{ \"error\": \"Invalid JSON content in SEO settings\" }";
            }

            _resourceManager.RegisterHeadScript(new HtmlString($"<script type=\"application/ld+json\">\n{json}\n</script>"));

        }

        return null;
    }
}
