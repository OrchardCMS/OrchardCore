using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Environment.Shell;
using OrchardCore.Themes.TheAdmin.Models;

namespace OrchardCore.Themes.TheAdmin.Services;

/// <summary>
/// Resolves the scheme of TheAdmin for the current user: the one they picked, kept in a cookie, or else the site's
/// default scheme, or else the theme's own default, or else the first scheme. Every scheme lives in the
/// <see cref="AdminSchemeStore"/>.
/// </summary>
public sealed class AdminSchemeService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AdminSchemeStore _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSchemeService"/> class.
    /// </summary>
    public AdminSchemeService(
        IHttpContextAccessor httpContextAccessor,
        ShellSettings shellSettings,
        AdminSchemeStore store)
    {
        _httpContextAccessor = httpContextAccessor;
        _store = store;

        // Named per tenant, like the color mode cookie of the theme toggler.
        CookieName = $"{shellSettings.Name}-{TheAdminConstants.ThemeId}-scheme";
    }

    /// <summary>
    /// The name of the cookie holding the scheme the user picked: '{tenant}-TheAdmin-scheme'.
    /// </summary>
    public string CookieName { get; }

    /// <summary>
    /// The schemes, in the order of the pickers.
    /// </summary>
    public Task<IReadOnlyList<AdminScheme>> ListAsync()
        => _store.ListAsync();

    /// <summary>
    /// The scheme the user picked, or <see langword="null"/> when they follow the site's default (or picked a scheme
    /// that was deleted since).
    /// </summary>
    public async Task<string> GetUserSchemeAsync()
    {
        var request = _httpContextAccessor.HttpContext?.Request;

        if (request is null
            || !request.Cookies.TryGetValue(CookieName, out var value)
            || !AdminSchemeStore.IsValidId(value))
        {
            return null;
        }

        return await _store.FindAsync(value) is not null ? value : null;
    }

    /// <summary>
    /// The site's default scheme.
    /// </summary>
    public Task<string> GetSiteSchemeAsync()
        => _store.GetDefaultIdAsync();

    /// <summary>
    /// The value of the scheme attribute of the page: the user's scheme, or else the site's default.
    /// </summary>
    public async Task<string> GetSchemeAttributeAsync()
        => await GetUserSchemeAsync() ?? await GetSiteSchemeAsync();

    /// <summary>
    /// The URL of the stylesheet of the schemes, versioned with a short hash of its text so browsers keep it until a
    /// scheme or the code that writes it changes. The stylesheet is built once per change of the schemes and read from
    /// memory, not built per request.
    /// </summary>
    public async Task<string> GetStylesheetUrlAsync(IUrlHelper url)
    {
        ArgumentNullException.ThrowIfNull(url);

        var (_, version) = await _store.GetStylesheetAsync();

        return url.Content($"~/{AdminSchemeEndpoints.StylesheetPath}?v={version}");
    }
}
