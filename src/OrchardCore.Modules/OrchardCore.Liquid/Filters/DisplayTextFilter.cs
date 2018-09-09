using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;

namespace OrchardCore.Liquid.Filters
{
    public class DisplayTextFilter : ILiquidFilter
    {
        public Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var contentItem = input.ToObjectValue() as ContentItem;

            if (contentItem == null)
            {
                return Task.FromResult<FluidValue>(NilValue.Instance);
            }

            return Task.FromResult<FluidValue>(new StringValue(contentItem.DisplayText ?? ""));
        }
    }
}
