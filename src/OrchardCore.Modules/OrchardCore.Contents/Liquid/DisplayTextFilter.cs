using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    public class DisplayTextFilter : ILiquidFilter
    {
        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext ctx)
        {
            var contentItem = input.ToObjectValue() as ContentItem;

            if (contentItem == null)
            {
                return new ValueTask<FluidValue>(NilValue.Instance);
            }

            return new ValueTask<FluidValue>(new StringValue(contentItem.DisplayText ?? ""));
        }
    }
}
