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

        public DateUtcFilter(IClock clock)
        {
            _clock = clock;
        }

        public ValueTask<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
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
                        return new ValueTask<FluidValue>(StringValue.Create("null"));
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
                        return new ValueTask<FluidValue>(StringValue.Create("null"));
                }
            }

            return new ValueTask<FluidValue>(new ObjectValue(value));
        }
    }
}
