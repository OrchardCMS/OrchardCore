using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Liquid;

public class DataLocalizationFilter : ILiquidFilter
{
    private readonly IDataLocalizer D;

    public DataLocalizationFilter(IDataLocalizer dataLocalizer)
    {
        D = dataLocalizer;
    }

    public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext templateContext)
    {
        var name = input.ToStringValue();

        var context = arguments.At(0).ToStringValue();

        if (arguments.At(0).IsNil())
        {
            context = string.Empty;
        }

        return FluidValue.Create(D[name, context].Value, templateContext.Options);
    }
}
