using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Resolves the time zone of the site from its general settings, and the time zone of a user from their profile when
/// the <c>OrchardCore.Users.TimeZone</c> feature lets users choose one.
/// </summary>
public sealed class DataTimeZoneResolver : IDataTimeZoneResolver
{
    /// <summary>
    /// The feature that lets users choose their time zone.
    /// </summary>
    public const string UserTimeZoneFeatureId = "OrchardCore.Users.TimeZone";

    // The name the user time zone feature stores the time zone of a user under, in the properties of the user.
    private const string UserTimeZoneProperty = "UserTimeZone";

    private readonly ISiteService _siteService;
    private readonly IClock _clock;
    private readonly ShellDescriptor _shellDescriptor;
    private readonly IServiceProvider _serviceProvider;

    private ITimeZone _siteTimeZone;

    public DataTimeZoneResolver(
        ISiteService siteService,
        IClock clock,
        ShellDescriptor shellDescriptor,
        IServiceProvider serviceProvider)
    {
        _siteService = siteService;
        _clock = clock;
        _shellDescriptor = shellDescriptor;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public async Task<ITimeZone> GetSiteTimeZoneAsync()
    {
        if (_siteTimeZone is null)
        {
            var site = await _siteService.GetSiteSettingsAsync();

            _siteTimeZone = GetTimeZone(site.TimeZoneId) ?? _clock.GetSystemTimeZone();
        }

        return _siteTimeZone;
    }

    /// <inheritdoc/>
    public async Task<ITimeZone> GetUserTimeZoneAsync(ClaimsPrincipal user)
    {
        var userName = user?.Identity?.Name;

        if (!string.IsNullOrEmpty(userName) &&
            _shellDescriptor.Features.Any(feature => feature.Id == UserTimeZoneFeatureId) &&
            _serviceProvider.GetService<UserManager<IUser>>() is { } userManager &&
            await userManager.FindByNameAsync(userName) is User account &&
            GetTimeZone(account.Properties[UserTimeZoneProperty]?["TimeZoneId"]?.GetValue<string>()) is { } timeZone)
        {
            return timeZone;
        }

        return await GetSiteTimeZoneAsync();
    }

    /// <inheritdoc/>
    public ITimeZone GetTimeZone(string timeZoneId)
    {
        if (string.IsNullOrEmpty(timeZoneId))
        {
            return null;
        }

        var timeZone = _clock.GetTimeZone(timeZoneId);

        // The clock falls back to the time zone of the system for an unknown identifier, which would hide a typo.
        return string.Equals(timeZone?.TimeZoneId, timeZoneId, StringComparison.Ordinal) ? timeZone : null;
    }
}
