using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Liquid;

public static class DisplayTextFilter
{
    public static ValueTask<FluidValue> DisplayText(FluidValue input, FilterArguments _, TemplateContext context)
    {
        var contentItem = input.ToObjectValue(context) as ContentItem;

        if (contentItem == null)
        {
            return ValueTask.FromResult<FluidValue>(NilValue.Instance);
        }

        return ValueTask.FromResult<FluidValue>(StringValue.Create(contentItem.DisplayText ?? string.Empty));
    }
}
