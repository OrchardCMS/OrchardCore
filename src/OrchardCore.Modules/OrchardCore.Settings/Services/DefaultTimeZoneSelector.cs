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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultTimeZoneSelector(
            IDefaultTimeZoneService siteTimeZoneService,
            IHttpContextAccessor httpContextAccessor)
        {
            _siteTimeZoneService = siteTimeZoneService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TimeZoneSelectorResult> GetTimeZoneAsync()
        {
            string currentTimeZoneId = await _siteTimeZoneService.GetCurrentTimeZoneIdAsync();
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
