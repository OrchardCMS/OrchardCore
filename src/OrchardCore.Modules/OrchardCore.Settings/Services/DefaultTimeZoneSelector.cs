using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.TimeZone;

namespace OrchardCore.Settings.Services
{
    /// <summary>
    /// Provides the timezone defined in the site configuration for the current scope (request).
    /// The same <see cref="TimeZoneSelectorResult"/> is returned if called multiple times 
    /// during the same scope.
    /// </summary>
    public class DefaultTimeZoneSelector : ITimeZoneSelector
    {
        private readonly ISiteService _siteService;
        private Task<TimeZoneSelectorResult> _result;

        public DefaultTimeZoneSelector(ISiteService siteService)
        {
            this._siteService = siteService;
        }

        public Task<TimeZoneSelectorResult> GetTimeZoneAsync()
        {
            if (_result != null)
            {
                return _result;
            }

            return _result = GetTimeZoneFromSettingsAsync();
        }

        private async Task<TimeZoneSelectorResult> GetTimeZoneFromSettingsAsync()
        {
            var siteSettings = await _siteService.GetSiteSettingsAsync();

            if (String.IsNullOrEmpty(siteSettings.TimeZoneId))
            {
                return null;
            }

            return new TimeZoneSelectorResult
            {
                Priority = 0,
                Id = siteSettings.TimeZoneId
            };
        }
    }
}
