using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    public class DisplayUrlFilter : ILiquidFilter
    {
        private readonly AutorouteOptions _autorouteOptions;
        private readonly IContentManager _contentManager;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public DisplayUrlFilter(IOptions<AutorouteOptions> autorouteOptions, IContentManager contentManager, IUrlHelperFactory urlHelperFactory)
        {
            _autorouteOptions = autorouteOptions.Value;
            _contentManager = contentManager;
            _urlHelperFactory = urlHelperFactory;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext context)
        {
            var contentItem = input.ToObjectValue() as ContentItem;
            RouteValueDictionary routeValues;

            if (contentItem == null)
            {
                routeValues = new RouteValueDictionary(_autorouteOptions.GlobalRouteValues);
                if (string.IsNullOrEmpty(input.ToStringValue()))
                {
                    throw new ArgumentException("content_item_id is empty while invoking 'display_url'");
                }
                routeValues[_autorouteOptions.ContentItemIdKey] = input.ToStringValue();
            }
            else
            {
                var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);
                routeValues = contentItemMetadata.DisplayRouteValues;
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(context.ViewContext);

            var linkUrl = urlHelper.RouteUrl(routeValues);

            return new StringValue(linkUrl);
        }
    }
}
