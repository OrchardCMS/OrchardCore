using System;
using DotLiquid;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.Liquid.Drops;

namespace Orchard.Liquid.Filters
{
    public static class MetadataFilters
    {
        public static string DisplayText(Context context, object input)
        {
            var contentItem = (input as ContentItemDrop)?.ContentItem;

            if (contentItem == null)
            {
                throw new ArgumentException("Content Item was expected");
            }

            var serviceProvider = context.Environments[0]["ServiceProvider"] as IServiceProvider;

            if (serviceProvider == null)
            {
                throw new ArgumentNullException("ServiceProvider");
            }

            var contentManager = serviceProvider.GetRequiredService<IContentManager>();
            var contentItemMetadata = contentManager.PopulateAspect<ContentItemMetadata>(contentItem);

            return contentItemMetadata.DisplayText;
        }

        public static string DisplayUrl(Context context, object input)
        {
            var contentItem = (input as ContentItemDrop)?.ContentItem;

            if (contentItem == null)
            {
                throw new ArgumentException("Content Item was expected");
            }

            var serviceProvider = context.Environments[0]["ServiceProvider"] as IServiceProvider;

            if (serviceProvider == null)
            {
                throw new ArgumentNullException("ServiceProvider");
            }

            var urlHelper = (context.Environments[0]["Url"] as UrlDrop)?.Url;
            var contentManager = serviceProvider.GetRequiredService<IContentManager>();
            var contentItemMetadata = contentManager.PopulateAspect<ContentItemMetadata>(contentItem);

            return urlHelper.RouteUrl(contentItemMetadata.DisplayRouteValues);
        }

        public static IHtmlContent Raw(Context context, string input)
        {
            return new HtmlString(input);
        }
    }
}
