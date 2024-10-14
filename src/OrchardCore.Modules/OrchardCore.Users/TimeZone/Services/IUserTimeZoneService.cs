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
    ValueTask<ITimeZone> GetAsync(IUser user);

    /// <summary>
    /// Updates the time zone for the given user.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/>.</param>
    ValueTask UpdateAsync(IUser user);
}
