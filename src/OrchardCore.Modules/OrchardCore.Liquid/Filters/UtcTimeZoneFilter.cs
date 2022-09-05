using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Modules;

namespace OrchardCore.Liquid.Filters
{
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
                var stringValue = input.ToStringValue();

                if (stringValue == "now" || stringValue == "today")
                {
                    value = await _localClock.LocalNowAsync;
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
                switch (input.ToObjectValue())
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

            return new ObjectValue(await _localClock.ConvertToUtcAsync(value.DateTime));
        }
    }
}
