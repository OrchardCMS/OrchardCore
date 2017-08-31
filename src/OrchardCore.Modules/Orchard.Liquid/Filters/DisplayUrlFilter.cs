using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using Orchard.ContentManagement;

namespace Orchard.Liquid.Filters
{
    public class DisplayUrlFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;

        public DisplayUrlFilter(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var contentItem = input.ToObjectValue() as ContentItem;

            if (contentItem == null)
            {
                return Task.FromResult<FluidValue>(NilValue.Instance);
            }

            var contentItemMetadata = _contentManager.PopulateAspect<ContentItemMetadata>(contentItem);

            if (!ctx.AmbientValues.TryGetValue("UrlHelper", out var urlHelper))
            {
                throw new ArgumentException("UrlHelper missing while invoking 'display_url'");
            }

            return Task.FromResult<FluidValue>(new StringValue(((IUrlHelper)urlHelper).RouteUrl(contentItemMetadata.DisplayRouteValues)));
        }
    }
}
