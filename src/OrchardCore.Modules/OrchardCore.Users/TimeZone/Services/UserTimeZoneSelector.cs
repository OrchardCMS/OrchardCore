using Microsoft.AspNetCore.Http;
using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.Services;

/// <summary>
/// Provides the time zone defined for the currently logged-in user for the current scope (request).
/// </summary>
public class UserTimeZoneSelector : ITimeZoneSelector
{
    private readonly IUserTimeZoneService _userTimeZoneService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTimeZoneSelector(
        IUserTimeZoneService userTimeZoneService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userTimeZoneService = userTimeZoneService;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<TimeZoneSelectorResult> GetTimeZoneAsync()
    {
        var result = !_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated
            ? null
            : new TimeZoneSelectorResult
            {
                Priority = 100,
                TimeZoneId = async () =>
                {
                    var timeZone = await _userTimeZoneService.GetAsync(_httpContextAccessor.HttpContext.User.Identity.Name);

                    return timeZone?.TimeZoneId;
                },
            };

        return Task.FromResult(result);
    }
}
