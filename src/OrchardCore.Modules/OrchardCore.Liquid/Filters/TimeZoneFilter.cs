using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Modules;

namespace OrchardCore.Liquid.Filters
{
    public class TimeZoneFilter : ILiquidFilter
    {
        private readonly ILocalClock _localClock;

        public TimeZoneFilter(ILocalClock localClock)
        {
            _localClock = localClock;
        }

        public async Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            DateTimeOffset value = DateTimeOffset.MinValue;

            if (input.Type == FluidValues.String)
            {
                var stringValue = input.ToStringValue();

                if (stringValue == "now" || stringValue == "today")
                {
                    value = await _localClock.LocalNowAsync;
                }
                else
                {
                    if (!DateTimeOffset.TryParse(stringValue, context.CultureInfo, DateTimeStyles.AssumeUniversal, out value))
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

            return new ObjectValue(await _localClock.ConvertToLocalAsync(value));
        }
    }
}
