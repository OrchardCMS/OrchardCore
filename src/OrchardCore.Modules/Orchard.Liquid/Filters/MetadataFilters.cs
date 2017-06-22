using System;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using Orchard.ContentManagement;

namespace Orchard.Liquid.Filters
{
    public class MetadataFilters : ITemplateContextHandler
    {
        private readonly IContentManager _contentManager;

        public MetadataFilters(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public void OnTemplateProcessing(TemplateContext context)
        {
            context.Filters.AddFilter("display_text", (input, arguments, ctx) =>
            {
                var contentItem = input.ToObjectValue() as ContentItem;

                if (contentItem == null)
                {
                    throw new ArgumentException("A Content Item was expected");
                }

                var contentItemMetadata = _contentManager.PopulateAspect<ContentItemMetadata>(contentItem);

                return new StringValue(contentItemMetadata.DisplayText);
            });

            context.Filters.AddFilter("display_url", (input, arguments, ctx) =>
            {
                var contentItem = input.ToObjectValue() as ContentItem;

                if (contentItem == null)
                {
                    throw new ArgumentException("A Content Item was expected");
                }

                var contentItemMetadata = _contentManager.PopulateAspect<ContentItemMetadata>(contentItem);

                object urlHelper;

                if (!ctx.AmbientValues.TryGetValue("UrlHelper", out urlHelper))
                {
                    throw new ArgumentException("UrlHelper missing while invoking 'display_url'");
                }

                return new StringValue(((IUrlHelper)urlHelper).RouteUrl(contentItemMetadata.DisplayRouteValues));
            });
        }
    }
}
