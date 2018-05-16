using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.TimeZone;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Settings.Services
{
    /// <summary>
    /// Provides the timezone defined in the site configuration for the current scope (request).
    /// The same <see cref="TimeZoneSelectorResult"/> is returned if called multiple times 
    /// during the same scope.
    /// </summary>
    public class DefaultTimeZoneSelector : ITimeZoneSelector
    {
        private readonly IDefaultTimeZoneService _siteTimeZoneService;

        public DefaultTimeZoneSelector(
            IDefaultTimeZoneService siteTimeZoneService)
        {
            _siteTimeZoneService = siteTimeZoneService;
        }

        public async Task<TimeZoneSelectorResult> GetTimeZoneAsync()
        {
            var currentTimeZoneId = await _siteTimeZoneService.GetCurrentTimeZoneIdAsync();
            if (String.IsNullOrEmpty(currentTimeZoneId))
            {
                return null;
            }

            return new TimeZoneSelectorResult
            {
                Priority = 0,
                Id = currentTimeZoneId
            };
        }
    }
}
