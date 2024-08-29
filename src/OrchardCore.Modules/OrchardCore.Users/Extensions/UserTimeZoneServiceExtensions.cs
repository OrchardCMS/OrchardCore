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
    public static async ValueTask<ITimeZone> GetAsync(this IUserTimeZoneService userTimeZoneService)
    {
        ArgumentNullException.ThrowIfNull(nameof(userTimeZoneService));

        var currentUser = await GetCurrentUserAsync(userTimeZoneService.HttpContext);

        return await userTimeZoneService.GetAsync(currentUser);
    }

    /// <summary>
    /// Updates the time zone for currently logged-in user.
    /// </summary>
    /// <param name="userTimeZoneService">The <see cref="IUserTimeZoneService"/>.</param>
    public static async ValueTask UpdateAsync(this IUserTimeZoneService userTimeZoneService)
    {
        ArgumentNullException.ThrowIfNull(nameof(userTimeZoneService));

        var currentUser = await GetCurrentUserAsync(userTimeZoneService.HttpContext);

        await userTimeZoneService.UpdateAsync(currentUser);
    }

    private static async Task<IUser> GetCurrentUserAsync(HttpContext httpContext)
    {
        var userName = httpContext.User?.Identity?.Name;
        var userManager = httpContext.RequestServices.GetRequiredService<UserManager<IUser>>();

        return await userManager.FindByNameAsync(userName) as User;
    }
}
