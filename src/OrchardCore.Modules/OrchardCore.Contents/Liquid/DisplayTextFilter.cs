using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Liquid
{
    public static class DisplayTextFilter
    {
        public static ValueTask<FluidValue> DisplayText(FluidValue input, FilterArguments _1, TemplateContext _2)
        {
            if (input.ToObjectValue() is not ContentItem contentItem)
            {
                return new ValueTask<FluidValue>(NilValue.Instance);
            }

            return new ValueTask<FluidValue>(new StringValue(contentItem.DisplayText ?? ""));
        }
    }
}
