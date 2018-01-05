using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;

namespace OrchardCore.Liquid.Filters
{
    public class DisplayTextFilter : ILiquidFilter
    {
        private readonly IContentManager _contentManager;

        public DisplayTextFilter(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var contentItem = input.ToObjectValue() as ContentItem;

            if (contentItem == null)
            {
                return NilValue.Instance;
            }

            var contentItemMetadata = await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem);

            return new StringValue(contentItemMetadata.DisplayText ?? "");
        }
    }
}
