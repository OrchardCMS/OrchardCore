using System.Security.Claims;
using OrchardCore.Modules;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Resolves the time zones data is converted to and from, such as by the <c>ConvertTimeZone</c> step. Unlike
/// <see cref="ILocalClock"/>, which uses the time zone of the current request, it resolves the same time zone in a
/// request and in a background run.
/// </summary>
public interface IDataTimeZoneResolver
{
    /// <summary>
    /// Gets the time zone of the site, from its general settings.
    /// </summary>
    /// <returns>The time zone of the site, or the time zone of the system when the site has none.</returns>
    Task<ITimeZone> GetSiteTimeZoneAsync();

    /// <summary>
    /// Gets the time zone a user chose in their profile.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    /// The time zone of the user, or the time zone of the site when the user chose none, or users can't choose one.
    /// </returns>
    Task<ITimeZone> GetUserTimeZoneAsync(ClaimsPrincipal user);

    /// <summary>
    /// Gets a time zone by its identifier.
    /// </summary>
    /// <param name="timeZoneId">The IANA identifier of the time zone, such as <c>America/New_York</c>.</param>
    /// <returns>The time zone, or <see langword="null"/> when there is no time zone with this identifier.</returns>
    ITimeZone GetTimeZone(string timeZoneId);
}
