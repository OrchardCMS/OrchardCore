using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.ResourceManagement;
using OrchardCore.Seo.Models;
using OrchardCore.Shortcodes.Services;
using Shortcodes;

namespace OrchardCore.Seo.Drivers
{
    public class SeoContentDriver : ContentDisplayDriver
    {       
        private readonly IContentManager _contentManager;
        private readonly IPageTitleBuilder _pageTitleBuilder;
        private readonly IResourceManager _resourceManager;
        private readonly IShortcodeService _shortcodeService;

        private bool _primaryContentRendered { get; set; }

        public SeoContentDriver(
            IContentManager contentManager,
            IPageTitleBuilder pageTitleBuilder,
            IResourceManager resourceManager,
            IShortcodeService shortcodeService
            )
        {
            _contentManager = contentManager;
            _pageTitleBuilder = pageTitleBuilder;
            _resourceManager = resourceManager;
            _shortcodeService = shortcodeService;
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
            if (!String.Equals(context.DisplayType, "Detail", StringComparison.OrdinalIgnoreCase) || context.Shape.TryGetProperty(nameof(ContentTypeSettings.Stereotype), out string stereotype))
            {
                return null;
            }

            var aspect = await _contentManager.PopulateAspectAsync<SeoAspect>(contentItem);
            
            if (!aspect.Render)
            {
                return null;
            }

            if (!String.IsNullOrEmpty(aspect.PageTitle))
            {
                _pageTitleBuilder.SetFixedTitle(new HtmlString(await RenderAsync(aspect.PageTitle, contentItem)));
            }

            if (!String.IsNullOrEmpty(aspect.MetaDescription))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "description",
                    Content = await RenderAsync(aspect.MetaDescription, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.MetaKeywords))
            {                   
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "keywords",
                    Content = await RenderAsync(aspect.MetaKeywords, contentItem)
                });
            }    

            if (!String.IsNullOrEmpty(aspect.Canonical))
            {
                _resourceManager.RegisterLink(new LinkEntry
                {
                    Href = aspect.Canonical,
                    Rel = "canonical"
                });
            }

            if (!String.IsNullOrEmpty(aspect.MetaRobots))
            {                   
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "robots",
                    Content = await RenderAsync(aspect.MetaRobots, contentItem)
                });
            }                        

            foreach(var customMetaTag in aspect.CustomMetaTags)
            {
                // Generate a new meta entry as the builder is preopulated.
                _resourceManager.RegisterMeta(new MetaEntry(
                    await RenderAsync(customMetaTag.Name, contentItem), 
                    await RenderAsync(customMetaTag.Property, contentItem), 
                    await RenderAsync(customMetaTag.Content, contentItem), 
                    await RenderAsync(customMetaTag.HttpEquiv, contentItem),
                    await RenderAsync(customMetaTag.Charset, contentItem)));
            }

            // OpenGraph.
            if (!String.IsNullOrEmpty(aspect.OpenGraphType))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:type", 
                    Content = await RenderAsync(aspect.OpenGraphType, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.OpenGraphTitle))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:title", 
                    Content = await RenderAsync(aspect.OpenGraphTitle, contentItem)
                });
            }  

            if (!String.IsNullOrEmpty(aspect.OpenGraphDescription))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:description", 
                    Content = await RenderAsync(aspect.OpenGraphDescription, contentItem)
                });
            }  
            
            if (!String.IsNullOrEmpty(aspect.OpenGraphImage))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:image", 
                    Content = await RenderAsync(aspect.OpenGraphImage, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.OpenGraphImageAlt))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:image:alt", 
                    Content = await RenderAsync(aspect.OpenGraphImageAlt, contentItem)
                });
            }  

            if (!String.IsNullOrEmpty(aspect.OpenGraphUrl))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:url", 
                    Content = await RenderAsync(aspect.OpenGraphUrl, contentItem)
                });
            }   

            if (!String.IsNullOrEmpty(aspect.OpenGraphSiteName))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:site_name", 
                    Content = await RenderAsync(aspect.OpenGraphSiteName, contentItem)
                });
            }       

            if (!String.IsNullOrEmpty(aspect.OpenGraphAppId))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "fb:app_id", 
                    Content = await RenderAsync(aspect.OpenGraphAppId, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.OpenGraphLocale))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:locale", 
                    Content = await RenderAsync(aspect.OpenGraphLocale, contentItem)
                });
            }

            // Twitter.
            if (!String.IsNullOrEmpty(aspect.TwitterCard))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "twitter:card", 
                    Content = await RenderAsync(aspect.TwitterCard, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.TwitterSite))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "twitter:site", 
                    Content = await RenderAsync(aspect.TwitterSite, contentItem)
                });
            } 

            if (!String.IsNullOrEmpty(aspect.TwitterTitle))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:title", 
                    Content = await RenderAsync(aspect.TwitterTitle, contentItem)
                });
            }     

            if (!String.IsNullOrEmpty(aspect.TwitterDescription))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:description", 
                    Content = await RenderAsync(aspect.TwitterDescription, contentItem)
                });
            }                                    

            if (!String.IsNullOrEmpty(aspect.TwitterImage))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:image",
                    Content = await RenderAsync(aspect.TwitterImage, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.TwitterImageAlt))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:image:alt",
                    Content = await RenderAsync(aspect.TwitterImageAlt, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.TwitterCreator))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:creator", 
                    Content = await RenderAsync(aspect.TwitterCreator, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.TwitterUrl))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:url", 
                    Content = await RenderAsync(aspect.TwitterUrl, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.GoogleSchema))
            {
                _resourceManager.RegisterHeadScript(new HtmlString($"<script type=\"application/ld+json\">\n{aspect.GoogleSchema}\n</script>"));
            }

            return null;
        }

        private ValueTask<string> RenderAsync(string template, ContentItem contentItem)
            => _shortcodeService.ProcessAsync(template,
                    new Context
                    {
                        ["ContentItem"] = contentItem
                    });
    }
}
