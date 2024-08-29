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
    private static HttpContext HttpContext => new HttpContextAccessor().HttpContext;

    /// <summary>
    /// Gets the time zone for currently logged-in user.
    /// </summary>
    /// <param name="userTimeZoneService">The <see cref="IUserTimeZoneService"/>.</param>
    public static async ValueTask<ITimeZone> GetAsync(this IUserTimeZoneService userTimeZoneService)
    {
        ArgumentNullException.ThrowIfNull(nameof(userTimeZoneService));

        var currentUser = await GetCurrentUserAsync();

        return await userTimeZoneService.GetAsync(currentUser);
    }

    /// <summary>
    /// Updates the time zone for currently logged-in user.
    /// </summary>
    /// <param name="userTimeZoneService">The <see cref="IUserTimeZoneService"/>.</param>
    public static async ValueTask UpdateAsync(this IUserTimeZoneService userTimeZoneService)
    {
        ArgumentNullException.ThrowIfNull(nameof(userTimeZoneService));

        var currentUser = await GetCurrentUserAsync();

        await userTimeZoneService.UpdateAsync(currentUser);
    }

    private static async Task<IUser> GetCurrentUserAsync()
    {
        var userName = HttpContext.User?.Identity?.Name;
        var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<IUser>>();

        return await userManager.FindByNameAsync(userName) as User;
    }
}
