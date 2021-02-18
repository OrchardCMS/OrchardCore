using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using OrchardCore.Seo.Models;
using Microsoft.AspNetCore.Html;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Title;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Seo.Drivers
{
    public class SeoContentDriver : ContentDisplayDriver
    {       
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly HtmlEncoder _htmlEncoder;

        private readonly IContentManager _contentManager;
        private readonly SeoPageTitleBuilder _seoPageTitleBuilder;
        private readonly IResourceManager _resourceManager;

        private bool _primaryContentRendered { get; set; }

        public SeoContentDriver(
            ILiquidTemplateManager liquidTemplateManager,
            HtmlEncoder htmlEncoder,
            IContentManager contentManager,
            IPageTitleBuilder pageTitleBuilder,
            IResourceManager resourceManager
            )
        {
            _contentManager = contentManager;
            _seoPageTitleBuilder = pageTitleBuilder as SeoPageTitleBuilder;
            _resourceManager = resourceManager;
            _liquidTemplateManager = liquidTemplateManager;
            _htmlEncoder = htmlEncoder;
        }

        public override async Task<IDisplayResult> DisplayAsync(ContentItem contentItem, BuildDisplayContext context)
        {
            // We only apply this on the primary content item, which is considered the first call to BuildDisplay
            if (_primaryContentRendered)
            {
                return null;
            }

            _primaryContentRendered = true;

            if (!String.Equals(context.DisplayType, "Detail", StringComparison.OrdinalIgnoreCase))
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
                _seoPageTitleBuilder.SetTitle(await RenderLiquidAsync(aspect.PageTitle, contentItem));
            }

            if (!String.IsNullOrEmpty(aspect.MetaDescription))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "description",
                    Content = await RenderLiquidAsync(aspect.MetaDescription, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.MetaKeywords))
            {                   
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "keywords",
                    Content = await RenderLiquidAsync(aspect.MetaKeywords, contentItem)
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
                    Content = await RenderLiquidAsync(aspect.MetaRobots, contentItem)
                });
            }                        

            foreach(var customMetaTag in aspect.CustomMetaTags)
            {
                // Generate a new meta entry as the builder is preopulated.
                _resourceManager.RegisterMeta(new MetaEntry(
                    await RenderLiquidAsync(customMetaTag.Name, contentItem), 
                    await RenderLiquidAsync(customMetaTag.Property, contentItem), 
                    await RenderLiquidAsync(customMetaTag.Content, contentItem), 
                    await RenderLiquidAsync(customMetaTag.HttpEquiv, contentItem),
                    await RenderLiquidAsync(customMetaTag.Charset, contentItem)));
            }

            // OpenGraph

            if (!String.IsNullOrEmpty(aspect.OpenGraphType))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:type", 
                    Content = await RenderLiquidAsync(aspect.OpenGraphType, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.OpenGraphTitle))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:title", 
                    Content = await RenderLiquidAsync(aspect.OpenGraphTitle, contentItem)
                });
            }  

            if (!String.IsNullOrEmpty(aspect.OpenGraphDescription))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:description", 
                    Content = await RenderLiquidAsync(aspect.OpenGraphDescription, contentItem)
                });
            }  
            
            if (!String.IsNullOrEmpty(aspect.OpenGraphImage))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:image", 
                    Content = await RenderLiquidAsync(aspect.OpenGraphImage, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.OpenGraphImageAlt))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:image:alt", 
                    Content = await RenderLiquidAsync(aspect.OpenGraphImageAlt, contentItem)
                });
            }  

            if (!String.IsNullOrEmpty(aspect.OpenGraphUrl))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:url", 
                    Content = await RenderLiquidAsync(aspect.OpenGraphUrl, contentItem)
                });
            }   

            if (!String.IsNullOrEmpty(aspect.OpenGraphSiteName))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:site_name", 
                    Content = await RenderLiquidAsync(aspect.OpenGraphSiteName, contentItem)
                });
            }       

            if (!String.IsNullOrEmpty(aspect.OpenGraphAppId))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "fb:app_id", 
                    Content = await RenderLiquidAsync(aspect.OpenGraphAppId, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.OpenGraphLocale))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "og:locale", 
                    Content = await RenderLiquidAsync(aspect.OpenGraphLocale, contentItem)
                });
            }

            // Twitter

            if (!String.IsNullOrEmpty(aspect.TwitterCard))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "twitter:card", 
                    Content = await RenderLiquidAsync(aspect.TwitterCard, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.TwitterSite))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Property = "twitter:site", 
                    Content = await RenderLiquidAsync(aspect.TwitterSite, contentItem)
                });
            } 

            if (!String.IsNullOrEmpty(aspect.TwitterTitle))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:title", 
                    Content = await RenderLiquidAsync(aspect.TwitterTitle, contentItem)
                });
            }     

            if (!String.IsNullOrEmpty(aspect.TwitterDescription))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:description", 
                    Content = await RenderLiquidAsync(aspect.TwitterDescription, contentItem)
                });
            }                                    

            if (!String.IsNullOrEmpty(aspect.TwitterImage))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:image",
                    Content = await RenderLiquidAsync(aspect.TwitterImage, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.TwitterImageAlt))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:image:alt",
                    Content = await RenderLiquidAsync(aspect.TwitterImageAlt, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.TwitterCreator))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:creator", 
                    Content = await RenderLiquidAsync(aspect.TwitterCreator, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.TwitterUrl))
            {
                _resourceManager.RegisterMeta(new MetaEntry
                {
                    Name = "twitter:url", 
                    Content = await RenderLiquidAsync(aspect.TwitterUrl, contentItem)
                });
            }

            if (!String.IsNullOrEmpty(aspect.GoogleSchema))
            {
                _resourceManager.RegisterHeadScript(new HtmlString($"<script type=\"application/ld+json\">\n{aspect.GoogleSchema}\n</script>"));
            }

            return null;
        }

        private Task<string> RenderLiquidAsync(string template, ContentItem contentItem)
            => _liquidTemplateManager.RenderAsync(template, _htmlEncoder, contentItem,
                                scope => scope.SetValue("ContentItem", contentItem));
    }
}
