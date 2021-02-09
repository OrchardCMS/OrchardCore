using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    public static class DisplayUrlFilter
    {
        public static async ValueTask<FluidValue> DisplayUrl(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var context = (LiquidTemplateContext)ctx;

            var contentItem = input.ToObjectValue() as ContentItem;
            RouteValueDictionary routeValues;

            if (contentItem == null)
            {
                var autoRouteOption = context.Services.GetRequiredService<IOptions<AutorouteOptions>>()?.Value;
                routeValues = new RouteValueDictionary(autoRouteOption.GlobalRouteValues);
                if (string.IsNullOrEmpty(input.ToStringValue()))
                {
                    throw new ArgumentException("content_item_id is empty while invoking 'display_url'");
                }
                routeValues[autoRouteOption.ContentItemIdKey] = input.ToStringValue();
            }
            else
            {
                var contentManager = context.Services.GetRequiredService<IContentManager>();
                var contentItemMetadata = await contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
                routeValues = contentItemMetadata.DisplayRouteValues;
            }

            var urlHelperFactory = context.Services.GetRequiredService<IUrlHelperFactory>();
            var urlHelper = urlHelperFactory.GetUrlHelper(context.ViewContext);

            var linkUrl = ((IUrlHelper)urlHelper).RouteUrl(routeValues);

            return new StringValue(linkUrl);
        }
    }
}
