using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.Services;

/// <summary>
/// Represents a service that responsible for managing the user's time zone settings.
/// </summary>
public interface IUserTimeZoneService
{
    /// <summary>
    /// Gets the time zone for the given user.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/>.</param>
    [Obsolete("This method is deprecated and will be removed in a future release. Please use GetAsync(userName) instead..")]
    ValueTask<ITimeZone> GetAsync(IUser user);

    /// <summary>
    /// Updates the time zone for the given user.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/>.</param>
    [Obsolete("This method is obsolete and will be removed in a future release. Cache invalidation is now handled automatically.")]
    ValueTask UpdateAsync(IUser user);

    /// <summary>
    /// Gets the time zone for the given user.
    /// </summary>
    /// <param name="userName">The user name.</param>
    ValueTask<ITimeZone> GetAsync(string userName)
        => ValueTask.FromResult<ITimeZone>(null);
}
