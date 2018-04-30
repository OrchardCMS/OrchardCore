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
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TimeZoneFilter(ISiteService siteService, IClock clock, IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _siteService = siteService;
            _clock = clock;
            _userService = userService;
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
            var user = await _userService.GetAuthenticatedUserAsync(_httpContextAccessor.HttpContext.User) as User;

            if (user?.TimeZone != null)
            {
                var userTimeZone = _clock.GetLocalTimeZone(user.TimeZone);
                return new ObjectValue(_clock.ConvertToTimeZone(value, userTimeZone));
            }
            else
            {
                var siteTimeZone = _clock.GetLocalTimeZone(siteSettings.TimeZone);
                return new ObjectValue(_clock.ConvertToTimeZone(value, siteTimeZone));
            }
        }
    }
}
