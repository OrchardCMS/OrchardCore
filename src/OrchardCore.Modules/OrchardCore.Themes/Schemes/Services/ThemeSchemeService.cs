using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Environment.Shell;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes.Services;

/// <summary>
/// Resolves the scheme of a theme for the current user: the one they picked, kept in a cookie per theme, or else the
/// site's default scheme of the theme, or else the theme's own default, or else its first scheme. Every scheme lives
/// in the <see cref="ThemeSchemeStore"/>.
/// </summary>
public sealed class ThemeSchemeService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ThemeSchemeStore _store;
    private readonly string _tenant;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeSchemeService"/> class.
    /// </summary>
    public ThemeSchemeService(
        IHttpContextAccessor httpContextAccessor,
        ShellSettings shellSettings,
        ThemeSchemeStore store)
    {
        _httpContextAccessor = httpContextAccessor;
        _store = store;
        _tenant = shellSettings.Name;
    }

    /// <summary>
    /// The name of the cookie holding the scheme the user picked for the theme: '{tenant}-{themeId}-scheme', named per
    /// tenant like the color mode cookie of the theme toggler.
    /// </summary>
    public string CookieName(string themeId)
        => $"{_tenant}-{_store.GetThemeId(themeId)}-scheme";

    /// <summary>
    /// The schemes of the theme, in the order of the pickers.
    /// </summary>
    public Task<IReadOnlyList<ThemeScheme>> ListAsync(string themeId)
        => _store.ListAsync(themeId);

    /// <summary>
    /// The scheme the user picked for the theme, or <see langword="null"/> when they follow the site's default (or
    /// picked a scheme that was deleted since).
    /// </summary>
    public async Task<string> GetUserSchemeAsync(string themeId)
    {
        var request = _httpContextAccessor.HttpContext?.Request;

        if (request is null
            || !_store.IsRegistered(themeId)
            || !request.Cookies.TryGetValue(CookieName(themeId), out var value)
            || !ThemeSchemeStore.IsValidId(value))
        {
            return null;
        }

        return await _store.FindAsync(themeId, value) is not null ? value : null;
    }

    /// <summary>
    /// The site's default scheme of the theme, or <see langword="null"/> when the theme has no scheme at all.
    /// </summary>
    public Task<string> GetSiteSchemeAsync(string themeId)
        => _store.GetDefaultIdAsync(themeId);

    /// <summary>
    /// The value of the theme's scheme attribute on the page: the user's scheme, or else the site's default, or
    /// <see langword="null"/> (Razor then leaves the attribute out, and the theme's stylesheet keeps its own fallback)
    /// when the theme has no scheme at all.
    /// </summary>
    public async Task<string> GetSchemeAttributeAsync(string themeId)
        => await GetUserSchemeAsync(themeId) ?? await GetSiteSchemeAsync(themeId);

    /// <summary>
    /// The URL of the stylesheet of the theme's schemes, versioned so browsers keep it until a scheme changes, or
    /// <see langword="null"/> when the theme has no scheme.
    /// </summary>
    public async Task<string> GetStylesheetUrlAsync(string themeId, IUrlHelper url)
    {
        ArgumentNullException.ThrowIfNull(url);

        if (!_store.IsRegistered(themeId))
        {
            return null;
        }

        var document = await _store.GetAsync(themeId);

        if (!document.Themes.TryGetValue(themeId, out var schemes) || schemes is null || schemes.Count == 0)
        {
            return null;
        }

        return url.Content($"~/{ThemeSchemeEndpoints.GetStylesheetPath(themeId)}?v={Uri.EscapeDataString(document.Identifier ?? string.Empty)}");
    }
}
