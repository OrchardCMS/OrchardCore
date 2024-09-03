using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Liquid;

public static class DisplayTextFilter
{
    public static ValueTask<FluidValue> DisplayText(FluidValue input, FilterArguments _1, TemplateContext _2)
    {
        var contentItem = input.ToObjectValue() as ContentItem;

        if (contentItem == null)
        {
            return ValueTask.FromResult<FluidValue>(NilValue.Instance);
        }

        return ValueTask.FromResult<FluidValue>(new StringValue(contentItem.DisplayText ?? string.Empty));
    }
}
