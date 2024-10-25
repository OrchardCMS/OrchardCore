using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.Services;

/// <summary>
/// Provides the time zone defined for the currently logged-in user for the current scope (request).
/// </summary>
public class UserTimeZoneSelector : ITimeZoneSelector
{
    private readonly IUserTimeZoneService _userTimeZoneService;
    private readonly UserManager<IUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTimeZoneSelector(
        IUserTimeZoneService userTimeZoneService,
        UserManager<IUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userTimeZoneService = userTimeZoneService;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TimeZoneSelectorResult> GetTimeZoneAsync()
    {
        var currentUser = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);

        return currentUser == null
            ? null
            : new TimeZoneSelectorResult
            {
                Priority = 100,
                TimeZoneId = async () => (await _userTimeZoneService.GetAsync(currentUser))?.TimeZoneId
            };
    }
}
