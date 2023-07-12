using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Liquid
{
    public static class DisplayTextFilter
    {
#pragma warning disable IDE0060 // Remove unused parameter
        public static ValueTask<FluidValue> DisplayText(FluidValue input, FilterArguments arguments, TemplateContext ctx)
#pragma warning restore IDE0060 // Remove unused parameter
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
