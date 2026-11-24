using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Documents;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Schemes;

namespace OrchardCore.Themes.TheAdmin.Services;

/// <summary>
/// The schemes of TheAdmin, in the cached <see cref="AdminSchemesDocument"/>, and their stylesheet, built once per
/// version of the document and kept in memory with the hash that versions its URL.
/// </summary>
public sealed partial class AdminSchemeStore
{
    /// <summary>
    /// The prefix of the ids given to the schemes admins add.
    /// </summary>
    public const string IdPrefix = "custom-";

    private readonly IDocumentManager<AdminSchemesDocument> _documents;
    private readonly IMemoryCache _memoryCache;

    // The document this scope seeded or changed, read in place of the cached one, which is only refreshed once the scope
    // commits.
    private AdminSchemesDocument _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSchemeStore"/> class.
    /// </summary>
    public AdminSchemeStore(IDocumentManager<AdminSchemesDocument> documents, IMemoryCache memoryCache)
    {
        _documents = documents;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// The schemes, read from the cache, with the theme's own schemes seeded the first time; never change what it
    /// returns.
    /// </summary>
    public async Task<AdminSchemesDocument> GetAsync()
    {
        var document = _current ?? await _documents.GetOrCreateImmutableAsync();

        return document.Seeded ? document : await SeedAsync();
    }

    /// <summary>
    /// The schemes, in the order of the pickers.
    /// </summary>
    public async Task<IReadOnlyList<AdminScheme>> ListAsync()
        => [.. (await GetAsync()).Schemes.Values
            .OrderBy(scheme => scheme.Position)
            .ThenBy(scheme => scheme.Name, StringComparer.CurrentCultureIgnoreCase)];

    /// <summary>
    /// The scheme with the given id, or <see langword="null"/>.
    /// </summary>
    public async Task<AdminScheme> FindAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return (await GetAsync()).Schemes.GetValueOrDefault(id);
    }

    /// <summary>
    /// The site's default scheme: the one the site picked, or else the theme's default, or else the first one, or else
    /// the theme's fallback scheme when there is no scheme at all.
    /// </summary>
    public async Task<string> GetDefaultIdAsync()
    {
        var document = await GetAsync();

        if (document.DefaultSchemeId is { } picked && document.Schemes.ContainsKey(picked))
        {
            return picked;
        }

        if (document.Schemes.ContainsKey(TheAdminSchemes.DefaultId))
        {
            return TheAdminSchemes.DefaultId;
        }

        var list = await ListAsync();

        // Schemes all removed (outside the admin, which never deletes the default) fall back to the theme's default
        // scheme, which the stylesheet writes in their place.
        return list.Count > 0 ? list[0].Id : TheAdminSchemes.DefaultId;
    }

    /// <summary>
    /// Makes a scheme the site's default scheme; returns whether there is such a scheme.
    /// </summary>
    public async Task<bool> SetDefaultAsync(string id)
    {
        if (await FindAsync(id) is null)
        {
            return false;
        }

        var document = await GetMutableAsync();
        document.DefaultSchemeId = id;
        await _documents.UpdateAsync(document);

        return true;
    }

    /// <summary>
    /// A new id for a scheme.
    /// </summary>
    public static string NewId()
        => IdPrefix + IdGenerator.GenerateId();

    /// <summary>
    /// Whether <paramref name="id"/> can be the id of a scheme: lowercase letters, digits and dashes, starting with a
    /// letter, so it is safe as a CSS attribute value.
    /// </summary>
    public static bool IsValidId(string id)
        => !string.IsNullOrEmpty(id) && id.Length <= 64 && IdRegex().IsMatch(id);

    /// <summary>
    /// Adds or replaces a scheme.
    /// </summary>
    public async Task SaveAsync(AdminScheme scheme)
    {
        ArgumentNullException.ThrowIfNull(scheme);

        if (!IsValidId(scheme.Id))
        {
            throw new ArgumentException($"'{scheme.Id}' is not a valid scheme id.", nameof(scheme));
        }

        var document = await GetMutableAsync();
        document.Schemes[scheme.Id] = scheme;
        await _documents.UpdateAsync(document);
    }

    /// <summary>
    /// The position after the last scheme, for a new one.
    /// </summary>
    public async Task<int> NextPositionAsync()
    {
        var schemes = await ListAsync();

        return schemes.Count == 0 ? 0 : schemes.Max(scheme => scheme.Position) + 1;
    }

    /// <summary>
    /// Removes a scheme; returns whether it was removed. The site's default scheme (see <see cref="GetDefaultIdAsync"/>)
    /// can't be removed: another scheme must be made the default first.
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        if (await FindAsync(id) is null || string.Equals(await GetDefaultIdAsync(), id, StringComparison.Ordinal))
        {
            return false;
        }

        var document = await GetMutableAsync();

        if (!document.Schemes.Remove(id))
        {
            return false;
        }

        await _documents.UpdateAsync(document);

        return true;
    }

    /// <summary>
    /// The stylesheet of the schemes, and its version: a short hash of its text, which changes when either the schemes
    /// (a save) or the code that writes them change. Built once per version of the document, then read from memory.
    /// </summary>
    public async Task<(string Css, string Version)> GetStylesheetAsync()
    {
        var document = await GetAsync();

        var stylesheet = await _memoryCache.GetOrCreateAsync($"AdminSchemes.Css:{document.Identifier}", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);

            // With no stored scheme, the stylesheet still holds the rules of the theme's fallback scheme, so the admin
            // never renders without its tokens.
            var stored = document.Schemes.Values;
            var css = AdminSchemeStylesheet.Build(stored.Count > 0 ? stored : [TheAdminSchemes.CreateFallback()]);

            return Task.FromResult(new CachedStylesheet(css, AdminSchemeStylesheet.ComputeVersion(css)));
        });

        return (stylesheet.Css, stylesheet.Version);
    }

    /// <summary>
    /// Whether the colors of a scheme are valid: the four main ones '#rgb' or '#rrggbb' colors, and the optional ones
    /// empty or such colors.
    /// </summary>
    public static bool HasValidColors(AdminScheme scheme)
    {
        ArgumentNullException.ThrowIfNull(scheme);

        return AdminColor.TryParse(scheme.Primary, out _)
            && AdminColor.TryParse(scheme.Deep, out _)
            && AdminColor.TryParse(scheme.Ink, out _)
            && AdminColor.TryParse(scheme.Accent, out _)
            && IsEmptyOrColor(scheme.OnPrimary)
            && IsEmptyOrColor(scheme.Page)
            && IsEmptyOrColor(scheme.Toolbar)
            && IsEmptyOrColor(scheme.Tint)
            && IsEmptyOrColor(scheme.TintDark);
    }

    /// <summary>
    /// Whether the typography and shape settings of a scheme are empty or valid: font names, and scales in their range.
    /// </summary>
    public static bool HasValidTypography(AdminScheme scheme)
    {
        ArgumentNullException.ThrowIfNull(scheme);

        return AdminSchemeTypography.IsValidFontFamily(scheme.FontFamily)
            && AdminSchemeTypography.IsValidFontFamily(scheme.HeadingFontFamily)
            && AdminSchemeTypography.IsValidFontScale(scheme.FontScale)
            && AdminSchemeTypography.IsValidRadiusScale(scheme.RadiusScale)
            && AdminSchemeTypography.IsValidDensity(scheme.Density);
    }

    private async Task<AdminSchemesDocument> SeedAsync()
    {
        var document = await _documents.GetOrCreateMutableAsync();

        if (!document.Seeded)
        {
            document.Seeded = true;

            foreach (var scheme in TheAdminSchemes.CreateDefaults())
            {
                document.Schemes.TryAdd(scheme.Id, scheme);
            }

            await _documents.UpdateAsync(document);
        }

        _current = document;

        return document;
    }

    private async Task<AdminSchemesDocument> GetMutableAsync()
    {
        // Seed first, so the theme's schemes are never added over what was saved before they were read.
        await GetAsync();

        var document = await _documents.GetOrCreateMutableAsync();

        // Later reads of this scope see the change before it is committed.
        _current = document;

        return document;
    }

    private static bool IsEmptyOrColor(string value)
        => string.IsNullOrWhiteSpace(value) || AdminColor.TryParse(value, out _);

    private sealed record CachedStylesheet(string Css, string Version);

    [GeneratedRegex("^[a-z][a-z0-9-]*$")]
    private static partial Regex IdRegex();
}
