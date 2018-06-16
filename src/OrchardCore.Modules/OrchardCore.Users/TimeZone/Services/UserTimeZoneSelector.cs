using System.Threading.Tasks;
using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.Services
{
    /// <summary>
    /// Provides the timezone defined for the currently logged-in user for the current scope (request).
    /// </summary>
    public class UserTimeZoneSelector : ITimeZoneSelector
    {
        private readonly UserTimeZoneService _userTimeZoneService;

        public UserTimeZoneSelector(UserTimeZoneService userTimeZoneService)
        {
            _userTimeZoneService = userTimeZoneService;
        }

        public Task<TimeZoneSelectorResult> GetTimeZoneAsync()
        {
            return Task.FromResult(new TimeZoneSelectorResult
            {
                Priority = 100,
                TimeZoneId = async () => (await _userTimeZoneService.GetUserTimeZoneAsync())?.TimeZoneId
            });
        }
    }
}
