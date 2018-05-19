using System.Threading.Tasks;
using OrchardCore.Modules;

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

        public Task<TimeZoneSelectorResult> GetTimeZoneAsync()
        {
            return Task.FromResult(new TimeZoneSelectorResult
            {
                Priority = 100,
                TimeZoneId = () => _userTimeZoneService.GetUserTimeZoneAsync().ContinueWith(x => x.Result?.TimeZoneId)
            });
        }

    }
}
