using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.AspNetCore.Http;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Liquid.Filters
{
    public class TimeZoneFilter : ILiquidFilter
    {
        private readonly IClock _clock;
        private readonly ISiteService _siteService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TimeZoneFilter(ISiteService siteService, IClock clock, IHttpContextAccessor httpContextAccessor)
        {
            _siteService = siteService;
            _clock = clock;
            _httpContextAccessor = httpContextAccessor;
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

            var siteSettings = await _siteService.GetSiteSettingsAsync();
            var siteTimeZone = _clock.GetLocalTimeZone(siteSettings.TimeZone);
            return new ObjectValue(_clock.ConvertToTimeZone(value, siteTimeZone));
        }
    }
}
