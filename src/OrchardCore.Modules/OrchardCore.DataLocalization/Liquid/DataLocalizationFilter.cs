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

        if (string.IsNullOrEmpty(context))
        {
            ArgumentException.ThrowIfNullOrEmpty(context, nameof(context));
        }

        if (arguments.Count > 1)
        {
            var parameters = new object[arguments.Count - 1];
            for (var i = 0; i < arguments.Count - 1; i++)
            {
                parameters[i] = arguments.At(i + 1).ToObjectValue();
            }

            return ValueTask.FromResult<FluidValue>(new StringValue(D[name, context, parameters]));
        }
        else
        {
            return ValueTask.FromResult<FluidValue>(new StringValue(D[name, context]));
        }
    }
}
