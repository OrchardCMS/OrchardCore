using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Liquid.Filters
{
    public class TimeZoneFilter : ILiquidFilter
    {
        private readonly ISiteService _siteService;
        private readonly IClock _clock;

        public TimeZoneFilter(ISiteService siteService, IClock clock)
        {
            _siteService = siteService;
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

            _clock.SetDateTimeZone((await _siteService.GetSiteSettingsAsync()).TimeZone);
            _clock.SetCulture(CultureInfo.CurrentCulture);
            //TODO : set culture of the clock from website settings
            return new ObjectValue(_clock.ToZonedDateTime(value).ToDateTimeOffset());
        }
    }
}
