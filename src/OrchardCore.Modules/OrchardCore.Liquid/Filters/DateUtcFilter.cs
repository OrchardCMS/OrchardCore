using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Modules;

namespace OrchardCore.Liquid.Filters
{
    public class DateUtcFilter : ILiquidFilter
    {
        private readonly IClock _clock;
        private readonly ILocalClock _localClock;

        public DateUtcFilter(IClock clock, ILocalClock localClock)
        {
            _clock = clock;
            _localClock = localClock;
        }

        public async ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            var value = DateTimeOffset.MinValue;

            if (input.Type == FluidValues.String)
            {
                var stringValue = input.ToStringValue();

                if (stringValue == "now" || stringValue == "today")
                {
                    value = _clock.UtcNow;
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
                        value = await _localClock.ConvertToUtcAsync(dateTime);
                        break;

                    case DateTimeOffset dateTimeOffset:
                        value = dateTimeOffset;
                        break;

                    default:
                        return NilValue.Instance;
                }
            }

            return new ObjectValue(value.UtcDateTime);
        }
    }
}
