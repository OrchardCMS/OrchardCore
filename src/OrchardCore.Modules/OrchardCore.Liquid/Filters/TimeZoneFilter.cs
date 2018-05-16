using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.TimeZone;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Liquid.Filters
{
    public class TimeZoneFilter : ILiquidFilter
    {
        private readonly ITimeZoneManager _timeZoneManager;
        private readonly IClock _clock;

        public TimeZoneFilter(ITimeZoneManager timeZoneManager, IClock clock)
        {
            _timeZoneManager = timeZoneManager;
            _clock = clock;
        }

        public async Task<FluidValue> ProcessAsync(FluidValue input, FilterArguments arguments, TemplateContext context)
        {
            DateTimeOffset value = DateTimeOffset.MinValue;

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
                        value = dateTime;
                        break;

                    case DateTimeOffset dateTimeOffset:
                        value = dateTimeOffset;
                        break;

                    default:
                        return NilValue.Instance;
                }
            }

            var timeZone = await _timeZoneManager.GetTimeZoneAsync();
            return new ObjectValue(_clock.ConvertToTimeZone(value, timeZone));
        }
    }
}
