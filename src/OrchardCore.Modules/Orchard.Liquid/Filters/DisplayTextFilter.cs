using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Orchard.ContentManagement;

namespace Orchard.Liquid.Filters
{
    public class DisplayTextFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;

        public DisplayTextFilter(IContentManager contentManager)
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

            return Task.FromResult<FluidValue>(new StringValue(contentItemMetadata.DisplayText ?? ""));
        }
    }
}
