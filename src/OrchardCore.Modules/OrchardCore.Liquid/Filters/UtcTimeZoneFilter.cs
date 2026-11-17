using System.Globalization;
using Fluid;
using Fluid.Values;
using OrchardCore.Modules;

namespace OrchardCore.Liquid.Filters;

public class UtcTimeZoneFilter : ILiquidFilter
{
    private readonly ILocalClock _localClock;

    public UtcTimeZoneFilter(ILocalClock localClock)
    {
        _localClock = localClock;
    }

    public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, LiquidTemplateContext ctx)
    {
        DateTimeOffset value;

        if (input.Type == FluidValues.String)
        {
            var stringValue = input.ToStringValue(ctx);

            if (stringValue == "now" || stringValue == "today")
            {
                value = await _localClock.GetLocalNowAsync();
            }
            else
            {
                if (!DateTimeOffset.TryParse(stringValue, ctx.Options.CultureInfo, DateTimeStyles.AssumeUniversal, out value))
                {
                    return NilValue.Instance;
                }
            }
        }
        else
        {
            switch (input.ToObjectValue(ctx))
            {
                case DateTime dateTime:
                    value = dateTime;
                    break;
                case DateTimeOffset dateTimeOffset:
                    value = dateTimeOffset;
                    break;
                default:
                    return NilValue.Instance;
            }
        }

        return ObjectValue.Create(await _localClock.ConvertToUtcAsync(value.DateTime));
    }
}
