using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.Services;

/// <summary>
/// Contract for user time zone service.
/// </summary>
public interface IUserTimeZoneService
{
    /// <summary>
    /// Gets the time zone of the currently logged-in user.
    /// </summary>
    public ValueTask<ITimeZone> GetTimeZoneAsync();

    /// <summary>
    /// Updates the time zone of the currently logged-in user.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/>.</param>
    public ValueTask UpdateTimeZoneAsync(IUser user);

    /// <summary>
    /// Gets the time zone identifier of the currently logged-in user.
    /// </summary>
    public ValueTask<string> GetTimeZoneIdAsync();
}
