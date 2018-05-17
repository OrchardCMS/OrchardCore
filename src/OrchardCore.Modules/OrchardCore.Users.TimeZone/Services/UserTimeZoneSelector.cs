using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.TimeZone;
using System;
using System.Threading.Tasks;

namespace OrchardCore.Users.TimeZone.Services
{
    /// <summary>
    /// Provides the timezone defined in the site configuration for the current scope (request).
    /// The same <see cref="TimeZoneSelectorResult"/> is returned if called multiple times 
    /// during the same scope.
    /// </summary>
    public class UserTimeZoneSelector : ITimeZoneSelector
    {
        private readonly IUserTimeZoneService _userTimeZoneService;

        public UserTimeZoneSelector(
            IUserTimeZoneService userTimeZoneService)
        {
            _userTimeZoneService = userTimeZoneService;
        }

        public async Task<TimeZoneSelectorResult> GetTimeZoneAsync()
        {
            var currentTimeZoneId = await _userTimeZoneService.GetCurrentTimeZoneIdAsync();
            if (String.IsNullOrEmpty(currentTimeZoneId))
            {
                return null;
            }

            return new TimeZoneSelectorResult
            {
                Priority = 100,
                Id = currentTimeZoneId
            };
        }
    }
}
