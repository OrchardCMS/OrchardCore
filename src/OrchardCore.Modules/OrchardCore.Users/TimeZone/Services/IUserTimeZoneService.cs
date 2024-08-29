using Microsoft.AspNetCore.Http;
using OrchardCore.Modules;

namespace OrchardCore.Users.TimeZone.Services;

/// <summary>
/// Contract for user time zone service.
/// </summary>
public interface IUserTimeZoneService
{
    /// <summary>
    /// Gets the current <see cref="HttpContext"/>.
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    /// Gets the time zone for the specified user.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/>.</param>
    public ValueTask<ITimeZone> GetTimeZoneAsync(IUser user);

    /// <summary>
    /// Updates the time zone for the specified user.
    /// </summary>
    /// <param name="user">The <see cref="IUser"/>.</param>
    public ValueTask UpdateTimeZoneAsync(IUser user);

    /// <summary>
    /// Gets the time zone identifier.
    /// </summary>
    public ValueTask<string> GetTimeZoneIdAsync();
}
