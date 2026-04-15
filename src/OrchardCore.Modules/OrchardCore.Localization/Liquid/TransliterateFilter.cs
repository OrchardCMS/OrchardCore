using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;
using OrchardCore.Localization;

namespace OrchardCore.localization.Liquid.Filters;

public class TransliterateFilter : ILiquidFilter
{
    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
    {
        var text = input.ToStringValue();

        if (string.IsNullOrEmpty(text))
        {
            return ValueTask.FromResult(input);
        }

        return ValueTask.FromResult<FluidValue>(StringValue.Create(text.Transliterate()));
    }
}
