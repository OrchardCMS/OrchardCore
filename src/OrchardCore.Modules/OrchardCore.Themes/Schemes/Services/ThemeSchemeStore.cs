using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Documents;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes.Services;

/// <summary>
/// The schemes of the themes that support them, in the cached <see cref="ThemeSchemesDocument"/>, and the stylesheet
/// of each theme, built by its <see cref="IThemeSchemeProvider"/> once per version of the document and kept in memory.
/// Reads of a theme that is not registered find nothing; writes to it throw an <see cref="ArgumentException"/>.
/// </summary>
public sealed partial class ThemeSchemeStore
{
    /// <summary>
    /// The prefix of the ids given to the schemes admins add.
    /// </summary>
    public const string IdPrefix = "custom-";

    private static readonly Dictionary<string, ThemeScheme> s_noSchemes = new(StringComparer.Ordinal);

    private readonly IDocumentManager<ThemeSchemesDocument> _documents;
    private readonly IMemoryCache _memoryCache;
    private readonly Dictionary<string, IThemeSchemeProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

    // The document this scope seeded, read in place of the cached one, which is only refreshed once the scope commits.
    private ThemeSchemesDocument _seeded;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeSchemeStore"/> class.
    /// </summary>
    public ThemeSchemeStore(
        IDocumentManager<ThemeSchemesDocument> documents,
        IEnumerable<IThemeSchemeProvider> providers,
        IMemoryCache memoryCache)
    {
        _documents = documents;
        _memoryCache = memoryCache;

        foreach (var provider in providers)
        {
            _providers.TryAdd(provider.ThemeId, provider);
        }
    }

    /// <summary>
    /// The ids of the themes that support schemes.
    /// </summary>
    public IEnumerable<string> ThemeIds
        => _providers.Keys;

    /// <summary>
    /// Whether the theme supports schemes.
    /// </summary>
    public bool IsRegistered(string themeId)
        => !string.IsNullOrEmpty(themeId) && _providers.ContainsKey(themeId);

    /// <summary>
    /// The provider of the theme, or <see langword="null"/> when the theme does not support schemes. The id is matched
    /// without regard to case.
    /// </summary>
    public IThemeSchemeProvider GetProvider(string themeId)
        => string.IsNullOrEmpty(themeId) ? null : _providers.GetValueOrDefault(themeId);

    /// <summary>
    /// The id of the theme as the theme spells it, whatever the case of <paramref name="themeId"/>; or
    /// <paramref name="themeId"/> itself when the theme does not support schemes. The stored schemes are keyed by it.
    /// </summary>
    public string GetThemeId(string themeId)
        => GetProvider(themeId)?.ThemeId ?? themeId;

    /// <summary>
    /// The schemes, read from the cache, with the defaults of the theme seeded the first time; never change what it
    /// returns.
    /// </summary>
    public async Task<ThemeSchemesDocument> GetAsync(string themeId)
    {
        themeId = GetThemeId(themeId);

        var document = _seeded ?? await _documents.GetOrCreateImmutableAsync();

        var provider = GetProvider(themeId);

        if (provider is null || document.Seeded.Contains(themeId))
        {
            return document;
        }

        return await SeedAsync(provider);
    }

    /// <summary>
    /// The schemes of the theme, in the order of the pickers.
    /// </summary>
    public async Task<IReadOnlyList<ThemeScheme>> ListAsync(string themeId)
    {
        themeId = GetThemeId(themeId);

        if (!IsRegistered(themeId))
        {
            return [];
        }

        return [.. GetSchemes(await GetAsync(themeId), themeId).Values
            .OrderBy(scheme => scheme.Position)
            .ThenBy(scheme => scheme.Name, StringComparer.CurrentCultureIgnoreCase)];
    }

    /// <summary>
    /// The scheme of the theme with the given id, or <see langword="null"/>.
    /// </summary>
    public async Task<ThemeScheme> FindAsync(string themeId, string id)
    {
        themeId = GetThemeId(themeId);

        if (string.IsNullOrEmpty(id) || !IsRegistered(themeId))
        {
            return null;
        }

        return GetSchemes(await GetAsync(themeId), themeId).GetValueOrDefault(id);
    }

    /// <summary>
    /// The site's default scheme of the theme: the one the site picked, or else the provider's default, or else the
    /// first one; <see langword="null"/> when the theme has no scheme.
    /// </summary>
    public async Task<string> GetDefaultIdAsync(string themeId)
    {
        themeId = GetThemeId(themeId);

        if (!IsRegistered(themeId))
        {
            return null;
        }

        var document = await GetAsync(themeId);
        var schemes = GetSchemes(document, themeId);

        if (document.Defaults.TryGetValue(themeId, out var picked) && picked is not null && schemes.ContainsKey(picked))
        {
            return picked;
        }

        var providerDefault = _providers[themeId].DefaultSchemeId;

        if (providerDefault is not null && schemes.ContainsKey(providerDefault))
        {
            return providerDefault;
        }

        var list = await ListAsync(themeId);

        return list.Count > 0 ? list[0].Id : null;
    }

    /// <summary>
    /// Makes a scheme the site's default scheme of the theme; returns whether there is such a scheme.
    /// </summary>
    public async Task<bool> SetDefaultAsync(string themeId, string id)
    {
        themeId = GetThemeId(themeId);

        EnsureRegistered(themeId);

        if (await FindAsync(themeId, id) is null)
        {
            return false;
        }

        var document = await GetMutableAsync(themeId);
        document.Defaults[themeId] = id;
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
    /// Adds or replaces a scheme of the theme.
    /// </summary>
    public async Task SaveAsync(string themeId, ThemeScheme scheme)
    {
        themeId = GetThemeId(themeId);

        ArgumentNullException.ThrowIfNull(scheme);
        EnsureRegistered(themeId);

        if (!IsValidId(scheme.Id))
        {
            throw new ArgumentException($"'{scheme.Id}' is not a valid scheme id.", nameof(scheme));
        }

        var document = await GetMutableAsync(themeId);
        GetOrAddSchemes(document, themeId)[scheme.Id] = scheme;
        await _documents.UpdateAsync(document);
    }

    /// <summary>
    /// The position after the last scheme of the theme, for a new one.
    /// </summary>
    public async Task<int> NextPositionAsync(string themeId)
    {
        themeId = GetThemeId(themeId);

        var schemes = await ListAsync(themeId);

        return schemes.Count == 0 ? 0 : schemes.Max(scheme => scheme.Position) + 1;
    }

    /// <summary>
    /// Removes a scheme of the theme; returns whether it was removed. The site's default scheme of the theme (see
    /// <see cref="GetDefaultIdAsync"/>) can't be removed: another scheme must be made the default first.
    /// </summary>
    public async Task<bool> DeleteAsync(string themeId, string id)
    {
        themeId = GetThemeId(themeId);

        EnsureRegistered(themeId);

        if (await FindAsync(themeId, id) is null || string.Equals(await GetDefaultIdAsync(themeId), id, StringComparison.Ordinal))
        {
            return false;
        }

        var document = await GetMutableAsync(themeId);

        if (!GetOrAddSchemes(document, themeId).Remove(id))
        {
            return false;
        }

        await _documents.UpdateAsync(document);

        return true;
    }

    /// <summary>
    /// The stylesheet of the schemes of the theme, and the version it was built from, which changes on every save.
    /// </summary>
    public async Task<(string Css, string Version)> GetStylesheetAsync(string themeId)
    {
        themeId = GetThemeId(themeId);

        EnsureRegistered(themeId);

        var document = await GetAsync(themeId);
        var version = document.Identifier ?? string.Empty;

        var css = await _memoryCache.GetOrCreateAsync($"ThemeSchemes.Css:{themeId}:{version}", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);

            return Task.FromResult(BuildStylesheet(_providers[themeId], GetSchemes(document, themeId).Values));
        });

        return (css, version);
    }

    /// <summary>
    /// The CSS of the given schemes, written by the theme's provider; the schemes with an invalid id are skipped.
    /// </summary>
    public static string BuildStylesheet(IThemeSchemeProvider provider, IEnumerable<ThemeScheme> schemes)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var css = new StringBuilder();

        foreach (var scheme in schemes)
        {
            if (scheme is not null && IsValidId(scheme.Id))
            {
                provider.AppendCss(css, scheme.Id, scheme);
            }
        }

        return css.ToString();
    }

    /// <summary>
    /// Whether the colors of a scheme are valid: the four main ones '#rgb' or '#rrggbb' colors, and the optional ones
    /// empty or such colors.
    /// </summary>
    public static bool HasValidColors(ThemeScheme scheme)
    {
        ArgumentNullException.ThrowIfNull(scheme);

        return ThemeColor.TryParse(scheme.Primary, out _)
            && ThemeColor.TryParse(scheme.Deep, out _)
            && ThemeColor.TryParse(scheme.Ink, out _)
            && ThemeColor.TryParse(scheme.Accent, out _)
            && IsEmptyOrColor(scheme.OnPrimary)
            && IsEmptyOrColor(scheme.Page)
            && IsEmptyOrColor(scheme.Toolbar)
            && IsEmptyOrColor(scheme.Tint)
            && IsEmptyOrColor(scheme.TintDark);
    }

    /// <summary>
    /// Whether the typography and shape settings of a scheme are empty or valid: known font families, and scales in
    /// their range.
    /// </summary>
    public static bool HasValidTypography(ThemeScheme scheme)
    {
        ArgumentNullException.ThrowIfNull(scheme);

        return ThemeSchemeTypography.IsValidFontFamily(scheme.FontFamily)
            && ThemeSchemeTypography.IsValidFontFamily(scheme.HeadingFontFamily)
            && ThemeSchemeTypography.IsValidFontScale(scheme.FontScale)
            && ThemeSchemeTypography.IsValidRadiusScale(scheme.RadiusScale)
            && ThemeSchemeTypography.IsValidDensity(scheme.Density);
    }

    private async Task<ThemeSchemesDocument> SeedAsync(IThemeSchemeProvider provider)
    {
        var document = await _documents.GetOrCreateMutableAsync();

        if (document.Seeded.Add(provider.ThemeId))
        {
            var schemes = GetOrAddSchemes(document, provider.ThemeId);

            foreach (var scheme in provider.CreateDefaults())
            {
                if (scheme is not null && IsValidId(scheme.Id))
                {
                    schemes.TryAdd(scheme.Id, scheme);
                }
            }

            await _documents.UpdateAsync(document);
        }

        _seeded = document;

        return document;
    }

    private async Task<ThemeSchemesDocument> GetMutableAsync(string themeId)
    {
        // Seed first, so a theme's defaults are never added over what was saved before they were read.
        await GetAsync(themeId);

        var document = await _documents.GetOrCreateMutableAsync();

        // Later reads of this scope see the change before it is committed.
        _seeded = document;

        return document;
    }

    private void EnsureRegistered(string themeId)
    {
        if (!IsRegistered(themeId))
        {
            throw new ArgumentException($"The theme '{themeId}' does not support schemes.", nameof(themeId));
        }
    }

    // Read only: never change what it returns.
    private static Dictionary<string, ThemeScheme> GetSchemes(ThemeSchemesDocument document, string themeId)
        => document.Themes.TryGetValue(themeId, out var schemes) && schemes is not null
            ? schemes
            : s_noSchemes;

    private static Dictionary<string, ThemeScheme> GetOrAddSchemes(ThemeSchemesDocument document, string themeId)
    {
        if (!document.Themes.TryGetValue(themeId, out var schemes) || schemes is null)
        {
            document.Themes[themeId] = schemes = new Dictionary<string, ThemeScheme>(StringComparer.Ordinal);
        }

        return schemes;
    }

    private static bool IsEmptyOrColor(string value)
        => string.IsNullOrWhiteSpace(value) || ThemeColor.TryParse(value, out _);

    [GeneratedRegex("^[a-z][a-z0-9-]*$")]
    private static partial Regex IdRegex();
}
