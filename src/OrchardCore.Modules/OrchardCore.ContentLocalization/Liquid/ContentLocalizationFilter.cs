using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.ContentLocalization.Liquid;

public class ContentLocalizationFilter : ILiquidFilter
{
    private readonly IContentLocalizationManager _contentLocalizationManager;

    public ContentLocalizationFilter(IContentLocalizationManager contentLocalizationManager)
    {
        _contentLocalizationManager = contentLocalizationManager;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
    {
        var locale = arguments.At(0).ToStringValue(ctx);

        if (arguments.At(0).IsNil())
        {
            locale = ctx.CultureInfo.Name;
        }

        if (input.Type == FluidValues.Array)
        {
            // List of content item ids

            var localizationSets = await input.EnumerateAsync(ctx).Select(x => x.ToStringValue(ctx)).ToArrayAsync();

            return FluidValue.Create(await _contentLocalizationManager.GetItemsForSetsAsync(localizationSets, locale), ctx.Options);
        }
        else
        {
            var localizationSet = input.ToStringValue(ctx);

            return FluidValue.Create(await _contentLocalizationManager.GetContentItemAsync(localizationSet, locale), ctx.Options);
        }
    }
}
