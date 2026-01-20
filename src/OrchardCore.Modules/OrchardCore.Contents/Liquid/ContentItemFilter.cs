using Fluid;
using Fluid.Values;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid;

public class ContentItemFilter : ILiquidFilter
{
    private readonly IContentManager _contentManager;

    public ContentItemFilter(IContentManager contentManager)
    {
        _contentManager = contentManager;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
    {
        if (input.Type == FluidValues.Array)
        {
            // List of content item ids to return.
            var contentItemIds = input.Enumerate(ctx).Select(x => x.ToStringValue());

            return FluidValue.Create(await _contentManager.GetAsync(contentItemIds), ctx.Options);
        }

        var contentItemId = input.ToStringValue();

        return FluidValue.Create(await _contentManager.GetAsync(contentItemId), ctx.Options);
    }
}
