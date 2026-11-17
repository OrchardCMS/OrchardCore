using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Documents;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Schemes;

namespace OrchardCore.Themes.TheAdmin.Services;

/// <summary>
/// The schemes of the admin, in the cached <see cref="AdminSchemesDocument"/>, and the stylesheet that carries their
/// tokens. The stylesheet is built once per version of the document and kept in memory.
/// </summary>
public sealed partial class AdminSchemeStore
{
    /// <summary>
    /// The prefix of the ids given to the schemes admins add.
    /// </summary>
    public const string IdPrefix = "custom-";

    private readonly IDocumentManager<AdminSchemesDocument> _documents;
    private readonly IMemoryCache _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSchemeStore"/> class.
    /// </summary>
    public AdminSchemeStore(IDocumentManager<AdminSchemesDocument> documents, IMemoryCache memoryCache)
    {
        _documents = documents;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// The schemes, read from the cache; never change what it returns.
    /// </summary>
    public Task<AdminSchemesDocument> GetAsync()
        => _documents.GetOrCreateImmutableAsync();

    /// <summary>
    /// The schemes in the order of the pickers.
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

        var document = await _documents.GetOrCreateMutableAsync();
        document.Schemes[scheme.Id] = scheme;
        await _documents.UpdateAsync(document);
    }

    /// <summary>
    /// The position after the last scheme, for a new one.
    /// </summary>
    public async Task<int> NextPositionAsync()
    {
        var schemes = (await GetAsync()).Schemes.Values;

        return schemes.Count == 0 ? 0 : schemes.Max(scheme => scheme.Position) + 1;
    }

    /// <summary>
    /// Adds the theme's default schemes that are not there yet (by id); used once, when the theme is enabled.
    /// </summary>
    public async Task SeedDefaultsAsync()
    {
        var document = await _documents.GetOrCreateMutableAsync();

        foreach (var scheme in AdminSchemes.CreateDefaults())
        {
            document.Schemes.TryAdd(scheme.Id, scheme);
        }

        await _documents.UpdateAsync(document);
    }

    /// <summary>
    /// Removes a scheme; returns whether there was one.
    /// </summary>
    public async Task<bool> DeleteAsync(string id)
    {
        var document = await _documents.GetOrCreateMutableAsync();

        if (!document.Schemes.Remove(id))
        {
            return false;
        }

        await _documents.UpdateAsync(document);

        return true;
    }

    /// <summary>
    /// The stylesheet of every scheme, and the version it was built from, which changes on every save.
    /// </summary>
    public async Task<(string Css, string Version)> GetStylesheetAsync()
    {
        var document = await GetAsync();
        var version = document.Identifier ?? string.Empty;

        var css = await _memoryCache.GetOrCreateAsync($"TheAdmin.Schemes.Css:{version}", entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromHours(1);

            return Task.FromResult(BuildStylesheet(document.Schemes.Values));
        });

        return (css, version);
    }

    /// <summary>
    /// The definition the tokens of a scheme are derived from, or <see langword="null"/> when a color is not valid.
    /// What the optional settings leave empty is worked out from the four colors or takes its default; an unknown font
    /// falls back on the default and a scale out of its range is clamped.
    /// </summary>
    public static AdminSchemeDefinition ToDefinition(AdminScheme scheme)
    {
        if (!AdminColor.TryParse(scheme.Primary, out var primary)
            || !AdminColor.TryParse(scheme.Deep, out var deep)
            || !AdminColor.TryParse(scheme.Ink, out var ink)
            || !AdminColor.TryParse(scheme.Accent, out var accent)
            || !TryParseOptional(scheme.OnPrimary, out var onPrimary)
            || !TryParseOptional(scheme.Page, out var page)
            || !TryParseOptional(scheme.Toolbar, out var toolbar)
            || !TryParseOptional(scheme.Tint, out var tint)
            || !TryParseOptional(scheme.TintDark, out var tintDark))
        {
            return null;
        }

        var derived = AdminSchemeDefinition.FromColors(primary, deep, ink, accent, scheme.LightMenu);

        return new AdminSchemeDefinition
        {
            Primary = primary,
            Deep = deep,
            Ink = ink,
            Accent = accent,
            LightMenu = scheme.LightMenu,
            Hue = scheme.Hue is { } hue ? ((hue % 360) + 360) % 360 : derived.Hue,
            Saturation = scheme.Saturation is { } saturation ? Math.Clamp(saturation, 0, 1) : derived.Saturation,
            OnPrimary = onPrimary ?? derived.OnPrimary,
            Page = page,
            Toolbar = toolbar,
            Tint = tint,
            TintDark = tintDark,
            FontFamily = AdminSchemeTypography.FindFont(scheme.FontFamily)?.Key ?? AdminSchemeTypography.DefaultFontFamily,
            HeadingFontFamily = AdminSchemeTypography.FindFont(scheme.HeadingFontFamily)?.Key,
            FontScale = Clamp(scheme.FontScale, AdminSchemeTypography.MinFontScale, AdminSchemeTypography.MaxFontScale, AdminSchemeTypography.DefaultFontScale),
            RadiusScale = Clamp(scheme.RadiusScale, AdminSchemeTypography.MinRadiusScale, AdminSchemeTypography.MaxRadiusScale, AdminSchemeTypography.DefaultRadiusScale),
            Density = Clamp(scheme.Density, AdminSchemeTypography.MinDensity, AdminSchemeTypography.MaxDensity, AdminSchemeTypography.DefaultDensity),
        };
    }

    /// <summary>
    /// Whether the typography and shape settings of a scheme are empty or valid: known font families, and scales in
    /// their range.
    /// </summary>
    public static bool HasValidTypography(AdminScheme scheme)
        => AdminSchemeTypography.IsValidFontFamily(scheme.FontFamily)
        && AdminSchemeTypography.IsValidFontFamily(scheme.HeadingFontFamily)
        && AdminSchemeTypography.IsValidFontScale(scheme.FontScale)
        && AdminSchemeTypography.IsValidRadiusScale(scheme.RadiusScale)
        && AdminSchemeTypography.IsValidDensity(scheme.Density);

    /// <summary>
    /// The CSS of the given schemes.
    /// </summary>
    public static string BuildStylesheet(IEnumerable<AdminScheme> schemes)
    {
        var css = new StringBuilder();

        foreach (var scheme in schemes)
        {
            var definition = ToDefinition(scheme);

            if (definition is not null && IsValidId(scheme.Id))
            {
                AdminSchemeCssBuilder.Append(css, scheme.Id, definition);
            }
        }

        return css.ToString();
    }

    private static double Clamp(double? value, double min, double max, double fallback)
        => value is { } number && double.IsFinite(number) ? Math.Clamp(number, min, max) : fallback;

    private static bool TryParseOptional(string value, out AdminColor? color)
    {
        color = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!AdminColor.TryParse(value, out var parsed))
        {
            return false;
        }

        color = parsed;

        return true;
    }

    [GeneratedRegex("^[a-z][a-z0-9-]*$")]
    private static partial Regex IdRegex();
}
