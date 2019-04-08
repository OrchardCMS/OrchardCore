using System;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;

namespace OrchardCore.Liquid.Filters
{
    public class EditorUrlFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;

        public EditorUrlFilter(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var contentItem = input.ToObjectValue() as ContentItem;

            if (contentItem == null)
            {
                return NilValue.Instance;
            }

            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);

            if (!ctx.AmbientValues.TryGetValue("UrlHelper", out var urlHelper))
            {
                throw new ArgumentException("UrlHelper missing while invoking 'editor_url'");
            }

            return new StringValue(((IUrlHelper)urlHelper).RouteUrl(contentItemMetadata.EditorRouteValues));
        }
    }
}
