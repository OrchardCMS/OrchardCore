using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Liquid.Filters
{
    public class DisplayUrlFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;

        public DisplayUrlFilter(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var isContentItemId = arguments["is_content_item_id"].Or(arguments.At(0)).ToBooleanValue();
            RouteValueDictionary routeValues;

            if (isContentItemId)
            {
                var autoRouteOption = ShellScope.Services.GetRequiredService<IOptions<AutorouteOptions>>()?.Value;
                routeValues = new RouteValueDictionary(autoRouteOption.GlobalRouteValues);
                if (string.IsNullOrEmpty(input.ToStringValue()))
                {
                    throw new ArgumentException("content_item_id is empty while invoking 'display_url'");
                }
                routeValues[autoRouteOption.ContentItemIdKey] = input.ToStringValue();
            }
            else
            {
                var contentItem = input.ToObjectValue() as ContentItem;

                if (contentItem == null)
                {
                    return NilValue.Instance;
                }

                var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
                routeValues = contentItemMetadata.DisplayRouteValues;
            }

            if (!ctx.AmbientValues.TryGetValue("UrlHelper", out var urlHelper))
            {
                throw new ArgumentException("UrlHelper missing while invoking 'display_url'");
            }

            var linkUrl = ((IUrlHelper)urlHelper).RouteUrl(routeValues);

            return new StringValue(linkUrl);
        }
    }
}
