using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Themes.TheAdmin.Models;

namespace OrchardCore.Themes.TheAdmin.Services;

/// <summary>
/// Resolves the scheme of the admin for the current user: the one they picked in the top bar, kept in a cookie, or
/// else the site's default scheme (<see cref="AdminThemeSettings.DefaultScheme"/>), or else Blue, or else the first
/// scheme there is. Every scheme lives in the <see cref="AdminSchemeStore"/>.
/// </summary>
public sealed class AdminSchemeService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISiteService _siteService;
    private readonly AdminSchemeStore _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSchemeService"/> class.
    /// </summary>
    public AdminSchemeService(
        IHttpContextAccessor httpContextAccessor,
        ISiteService siteService,
        ShellSettings shellSettings,
        AdminSchemeStore store)
    {
        _httpContextAccessor = httpContextAccessor;
        _siteService = siteService;
        _store = store;

        // Named per tenant, like the color mode cookie of the theme toggler ('{tenant}-admintheme').
        CookieName = $"{shellSettings.Name}-adminscheme";
    }

    /// <summary>
    /// The name of the cookie holding the scheme the user picked.
    /// </summary>
    public string CookieName { get; }

    /// <summary>
    /// Every scheme, in the order of the pickers.
    /// </summary>
    public Task<IReadOnlyList<AdminScheme>> ListAsync()
        => _store.ListAsync();

    /// <summary>
    /// The id of the scheme with the given id, or <see langword="null"/> when there is none.
    /// </summary>
    public async Task<string> FindAsync(string id)
        => AdminSchemeStore.IsValidId(id) && await _store.FindAsync(id) is not null ? id : null;

    /// <summary>
    /// The scheme the user picked, or <see langword="null"/> when they follow the site's default (or picked a scheme
    /// that was deleted since).
    /// </summary>
    public async Task<string> GetUserSchemeAsync()
    {
        var request = _httpContextAccessor.HttpContext?.Request;

        return request is not null && request.Cookies.TryGetValue(CookieName, out var value)
            ? await FindAsync(value)
            : null;
    }

    /// <summary>
    /// The site's default scheme, or <see langword="null"/> when there is no scheme at all.
    /// </summary>
    public async Task<string> GetSiteSchemeAsync()
    {
        var settings = await _siteService.GetSettingsAsync<AdminThemeSettings>();

        var scheme = await FindAsync(settings.DefaultScheme) ?? await FindAsync(AdminSchemes.DefaultId);

        if (scheme is not null)
        {
            return scheme;
        }

        var schemes = await _store.ListAsync();

        return schemes.Count > 0 ? schemes[0].Id : null;
    }

    /// <summary>
    /// The scheme the page is rendered in, or <see langword="null"/> when there is no scheme at all.
    /// </summary>
    public async Task<string> GetCurrentSchemeAsync()
        => await GetUserSchemeAsync() ?? await GetSiteSchemeAsync();

    /// <summary>
    /// The value of the page's 'data-ta-scheme' attribute: the current scheme, or <see langword="null"/> (Razor then
    /// leaves the attribute out and the stylesheet's own fallback colors apply) when there is no scheme at all.
    /// </summary>
    public Task<string> GetSchemeAttributeAsync()
        => GetCurrentSchemeAsync();

    /// <summary>
    /// The URL of the stylesheet of the schemes, versioned so browsers keep it until a scheme changes, or
    /// <see langword="null"/> when there is no scheme.
    /// </summary>
    public async Task<string> GetStylesheetUrlAsync(IUrlHelper url)
    {
        var document = await _store.GetAsync();

        if (document.Schemes.Count == 0)
        {
            return null;
        }

        return url.Content($"~/{AdminSchemeEndpoints.StylesheetPath}?v={Uri.EscapeDataString(document.Identifier ?? string.Empty)}");
    }
}
