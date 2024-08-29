using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.TimeZone.Services;

namespace OrchardCore.Users;

/// <summary>
/// Provides an extension for <see cref="IUserTimeZoneService"/>.
/// </summary>
public static class UserTimeZoneServiceExtensions
{
    /// <summary>
    /// Gets the time zone for currently logged-in user.
    /// </summary>
    /// <param name="userTimeZoneService">The <see cref="IUserTimeZoneService"/>.</param>
    public static async ValueTask<ITimeZone> GetTimeZoneAsync(this IUserTimeZoneService userTimeZoneService)
    {
        ArgumentNullException.ThrowIfNull(nameof(userTimeZoneService));

        var currentUser = await GetCurrentUserAsync(userTimeZoneService.HttpContext);

        return await userTimeZoneService.GetTimeZoneAsync(currentUser);
    }

    /// <summary>
    /// Updates the time zone for currently logged-in user.
    /// </summary>
    /// <param name="userTimeZoneService">The <see cref="IUserTimeZoneService"/>.</param>
    public static async ValueTask UpdateTimeZoneAsync(this IUserTimeZoneService userTimeZoneService)
    {
        ArgumentNullException.ThrowIfNull(nameof(userTimeZoneService));

        var currentUser = await GetCurrentUserAsync(userTimeZoneService.HttpContext);

        await userTimeZoneService.UpdateTimeZoneAsync(currentUser);
    }

    private static async Task<IUser> GetCurrentUserAsync(HttpContext httpContext)
    {
        var userName = httpContext.User?.Identity?.Name;
        var userManager = httpContext.RequestServices.GetRequiredService<UserManager<IUser>>();

        return await userManager.FindByNameAsync(userName) as User;
    }
}
