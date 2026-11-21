using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OrchardCore.Documents;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes.Services;

/// <summary>
/// The schemes of the themes that support them, in the cached <see cref="ThemeSchemesDocument"/>, and the stylesheet
/// of each theme, built from the theme's <see cref="ThemeSchemeOptions"/> once per version of the document and kept in
/// memory with the hash that versions its URL. Reads of a theme that is not registered find nothing; writes to it
/// throw an <see cref="ArgumentException"/>.
/// </summary>
public sealed partial class ThemeSchemeStore
{
    /// <summary>
    /// The prefix of the ids given to the schemes admins add.
    /// </summary>
    public const string IdPrefix = "custom-";

    private static readonly Dictionary<string, ThemeScheme> s_noSchemes = new(StringComparer.Ordinal);

    private readonly IDocumentManager<ThemeSchemesDocument> _documents;
    private readonly ThemeSchemesOptions _themes;
    private readonly IOptionsMonitor<ThemeSchemeOptions> _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _memoryCache;

    // The document this scope seeded, read in place of the cached one, which is only refreshed once the scope commits.
    private ThemeSchemesDocument _seeded;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeSchemeStore"/> class.
    /// </summary>
    public ThemeSchemeStore(
        IDocumentManager<ThemeSchemesDocument> documents,
        IOptions<ThemeSchemesOptions> themes,
        IOptionsMonitor<ThemeSchemeOptions> options,
        IServiceProvider serviceProvider,
        IMemoryCache memoryCache)
    {
        _documents = documents;
        _themes = themes.Value;
        _options = options;
        _serviceProvider = serviceProvider;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// The ids of the themes that support schemes.
    /// </summary>
    public IEnumerable<string> ThemeIds
        => _themes.Themes;

    /// <summary>
    /// Whether the theme supports schemes.
    /// </summary>
    public bool IsRegistered(string themeId)
        => !string.IsNullOrEmpty(themeId) && _themes.Themes.Contains(themeId);

    /// <summary>
    /// The options of the theme, or <see langword="null"/> when the theme does not support schemes. The id is matched
    /// without regard to case.
    /// </summary>
    public ThemeSchemeOptions GetOptions(string themeId)
        => IsRegistered(themeId) ? _options.Get(GetThemeId(themeId)) : null;

    /// <summary>
    /// The id of the theme as the theme spells it, whatever the case of <paramref name="themeId"/>; or
    /// <paramref name="themeId"/> itself when the theme does not support schemes. The stored schemes are keyed by it.
    /// </summary>
    public string GetThemeId(string themeId)
        => IsRegistered(themeId)
            ? _themes.Themes.First(id => string.Equals(id, themeId, StringComparison.OrdinalIgnoreCase))
            : themeId;

    /// <summary>
    /// The schemes, read from the cache, with the defaults of the theme seeded the first time; never change what it
    /// returns.
    /// </summary>
    public async Task<ThemeSchemesDocument> GetAsync(string themeId)
    {
        themeId = GetThemeId(themeId);

        var document = _seeded ?? await _documents.GetOrCreateImmutableAsync();

        if (!IsRegistered(themeId) || document.Seeded.Contains(themeId))
        {
            return document;
        }

        return await SeedAsync(themeId, _options.Get(themeId));
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
    /// The site's default scheme of the theme: the one the site picked, or else the theme's default, or else the
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

        var themeDefault = _options.Get(themeId).DefaultSchemeId;

        if (themeDefault is not null && schemes.ContainsKey(themeDefault))
        {
            return themeDefault;
        }

        var list = await ListAsync(themeId);

        // A theme whose schemes were all removed (outside the admin, which never deletes the default) falls back to the
        // scheme of its options, which the stylesheet writes in their place.
        return list.Count > 0 ? list[0].Id : _options.Get(themeId).GetFallbackScheme().Id;
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
    /// The stylesheet of the schemes of the theme, and its version: a short hash of its text, which changes when either
    /// the schemes (a save) or the code that writes them change. Built once per version of the document, then read
    /// from memory.
    /// </summary>
    public async Task<(string Css, string Version)> GetStylesheetAsync(string themeId)
    {
        themeId = GetThemeId(themeId);

        EnsureRegistered(themeId);

        var document = await GetAsync(themeId);

        var stylesheet = await _memoryCache.GetOrCreateAsync($"ThemeSchemes.Css:{themeId}:{document.Identifier}", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);

            // A theme with no stored scheme still gets the rules of its fallback scheme, so it never renders without its
            // tokens.
            var stored = GetSchemes(document, themeId).Values;
            var css = BuildStylesheet(themeId, stored.Count > 0 ? stored : [_options.Get(themeId).GetFallbackScheme()]);

            return Task.FromResult(new CachedStylesheet(css, ThemeSchemeStylesheet.ComputeVersion(css)));
        });

        return (stylesheet.Css, stylesheet.Version);
    }

    /// <summary>
    /// Builds the CSS of the given schemes with the theme's options and contributors; the schemes with an invalid id
    /// or an invalid color are skipped. Not cached, unlike <see cref="GetStylesheetAsync"/>.
    /// </summary>
    public string BuildStylesheet(string themeId, IEnumerable<ThemeScheme> schemes)
    {
        themeId = GetThemeId(themeId);

        EnsureRegistered(themeId);

        return ThemeSchemeStylesheet.Build(_options.Get(themeId), schemes, _serviceProvider);
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

    private async Task<ThemeSchemesDocument> SeedAsync(string themeId, ThemeSchemeOptions options)
    {
        var document = await _documents.GetOrCreateMutableAsync();

        if (document.Seeded.Add(themeId))
        {
            var schemes = GetOrAddSchemes(document, themeId);

            foreach (var scheme in options.GetSchemesToSeed())
            {
                if (scheme is not null && IsValidId(scheme.Id))
                {
                    // A copy: the options keep their own instances.
                    schemes.TryAdd(scheme.Id, JsonSerializer.Deserialize<ThemeScheme>(JsonSerializer.SerializeToUtf8Bytes(scheme)));
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

    private sealed record CachedStylesheet(string Css, string Version);

    [GeneratedRegex("^[a-z][a-z0-9-]*$")]
    private static partial Regex IdRegex();
}
