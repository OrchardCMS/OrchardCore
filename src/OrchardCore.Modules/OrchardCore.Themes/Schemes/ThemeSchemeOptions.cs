using System.Text;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes;

/// <summary>
/// How a theme supports schemes: the attribute of the page that holds the current scheme, the prefix of the tokens its
/// stylesheet writes, the schemes it seeds, and what else is written. One instance per theme, as named options keyed
/// by the theme id; configure it with
/// <see cref="ThemeSchemesServiceCollectionExtensions.AddThemeSchemes(Microsoft.Extensions.DependencyInjection.IServiceCollection, string, Action{ThemeSchemeOptions})"/>.
/// Every property has a default, so a theme can register with no configuration at all.
/// </summary>
public sealed class ThemeSchemeOptions
{
    /// <summary>
    /// The attribute of the page that holds the current scheme when a theme names none.
    /// </summary>
    public const string DefaultAttributeName = "data-scheme";

    /// <summary>
    /// The selector of the dark mode when a theme names none: Bootstrap 5.3's color mode attribute.
    /// </summary>
    public const string DefaultDarkModeSelector = "[data-bs-theme=\"dark\"]";

    /// <summary>
    /// The id of the scheme seeded for a theme that names no default schemes.
    /// </summary>
    public const string NeutralSchemeId = "default";

    private string _tokenPrefix;
    private string _defaultSchemeId;

    /// <summary>
    /// Gets or sets the id of the theme, the id of its extension (e.g. <c>TheAdmin</c>). Set by
    /// <see cref="ThemeSchemesServiceCollectionExtensions.AddThemeSchemes(Microsoft.Extensions.DependencyInjection.IServiceCollection, string, Action{ThemeSchemeOptions})"/>.
    /// </summary>
    public string ThemeId { get; set; }

    /// <summary>
    /// Gets or sets the attribute of the page's <c>&lt;html&gt;</c> element that holds the id of the current scheme,
    /// which every rule of the stylesheet selects on. Defaults to <c>data-scheme</c>.
    /// </summary>
    public string AttributeName { get; set; } = DefaultAttributeName;

    /// <summary>
    /// Gets or sets the prefix of the tokens: the stylesheet writes <c>--{prefix}-primary</c>, and so on. Defaults to
    /// the theme id in lowercase, with every character other than a letter or a digit turned into a dash (e.g.
    /// <c>mytheme</c> for <c>MyTheme</c>).
    /// </summary>
    public string TokenPrefix
    {
        get => string.IsNullOrEmpty(_tokenPrefix) ? CreateTokenPrefix(ThemeId) : _tokenPrefix;
        set => _tokenPrefix = value;
    }

    /// <summary>
    /// Gets or sets the id of the scheme used when the site names no default for the theme, or names one that was
    /// deleted. Defaults to the id of the first of <see cref="Defaults"/>, or to <see cref="NeutralSchemeId"/> when
    /// there are none.
    /// </summary>
    public string DefaultSchemeId
    {
        get => _defaultSchemeId ?? (Defaults is { Count: > 0 } ? Defaults[0]?.Id : NeutralSchemeId);
        set => _defaultSchemeId = value;
    }

    /// <summary>
    /// Gets or sets the schemes the theme seeds the first time its schemes are read. They are ordinary records from
    /// then on: admins edit or delete them, and add their own. When empty, one neutral blue scheme named "Default"
    /// (<see cref="CreateNeutralScheme"/>) is seeded.
    /// </summary>
    public IList<ThemeScheme> Defaults { get; set; } = [];

    /// <summary>
    /// Gets or sets the selector of the dark mode, which the dark rules of a scheme put before the scheme attribute's
    /// one: <c>{DarkModeSelector}[{AttributeName}="{id}"]</c>, both on the same element. Defaults to
    /// <c>[data-bs-theme="dark"]</c>, Bootstrap 5.3's color mode attribute. Set it to <see langword="null"/> for a theme
    /// with no dark mode: no dark rules are written.
    /// </summary>
    public string DarkModeSelector { get; set; } = DefaultDarkModeSelector;

    /// <summary>
    /// Gets the generic tokens the theme names otherwise, from the generic name (e.g. <c>deep</c>, without the prefix)
    /// to the theme's own (e.g. <c>navy</c>). The <c>-rgb</c> token of a renamed color follows its name.
    /// </summary>
    public IDictionary<string, string> TokenNames { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets the types of the <see cref="IThemeSchemeTokenContributor"/>s that add tokens and rules to the stylesheet,
    /// in order. They are created through dependency injection each time the stylesheet is built. See
    /// <see cref="AddTokens{T}"/> and <see cref="UseBootstrap"/>.
    /// </summary>
    public IList<Type> TokenContributors { get; } = [];

    /// <summary>
    /// Gets or sets what writes the whole CSS of a scheme in place of the generated tokens, for a theme that wants full
    /// control. It is called once per scheme whose colors are valid; <see cref="ThemeSchemeCssContext.WriteDefault"/>
    /// writes what would have been written without it. When <see langword="null"/> (the default), the generated tokens
    /// and the contributors' ones are written.
    /// </summary>
    public Action<ThemeSchemeCssContext> CssWriter { get; set; }

    /// <summary>
    /// Gets or sets the settings of a scheme, beyond its colors, that the theme's stylesheet reads: its fonts, font size,
    /// corners and density. The editor of schemes only shows these, and the stylesheet only writes their tokens.
    /// Defaults to <see cref="ThemeSchemeFeatures.None"/>, colors only; <see cref="UseBootstrap"/> adds the fonts and the
    /// corners, which Bootstrap's variables carry.
    /// </summary>
    public ThemeSchemeFeatures Features { get; set; }

    /// <summary>
    /// Gets or sets the fonts the theme ships (its <c>@font-face</c> rules), which the editor suggests and which a scheme
    /// names by their key or name. A scheme may also name any other font family, e.g. one installed on the visitor's
    /// device. The system font is always offered.
    /// </summary>
    public IList<ThemeSchemeFont> Fonts { get; set; } = [];

    /// <summary>
    /// Gets or sets the font of a scheme that names none: the key or the name of one of <see cref="Fonts"/>, or a list of
    /// font families. Defaults to the system font.
    /// </summary>
    public string DefaultFontFamily { get; set; } = ThemeSchemeTypography.SystemFont;

    /// <summary>
    /// Whether the theme supports every one of the given settings.
    /// </summary>
    public bool Supports(ThemeSchemeFeatures features)
        => (Features & features) == features;

    /// <summary>
    /// The CSS font stack of a font a scheme names, through the theme's <see cref="Fonts"/>; the stack of
    /// <see cref="DefaultFontFamily"/> when the scheme names none, or names one that is not valid.
    /// </summary>
    public string GetFontStack(string fontFamily)
        => ThemeSchemeTypography.GetFontStack(fontFamily, Fonts)
            ?? ThemeSchemeTypography.GetFontStack(DefaultFontFamily, Fonts)
            ?? ThemeSchemeTypography.System.Stack;

    /// <summary>
    /// Adds a contributor of tokens and rules, created through dependency injection, so it can take services. Adding
    /// the same type twice adds it once.
    /// </summary>
    public ThemeSchemeOptions AddTokens<T>()
        where T : class, IThemeSchemeTokenContributor
    {
        if (!TokenContributors.Contains(typeof(T)))
        {
            TokenContributors.Add(typeof(T));
        }

        return this;
    }

    /// <summary>
    /// Also writes Bootstrap 5.3's root variables (<c>--bs-primary</c>, <c>--bs-link-color</c>, <c>--bs-body-bg</c>,
    /// <c>--bs-border-radius</c>...) from the generated tokens, for a Bootstrap theme whose own CSS does not map the
    /// tokens itself, and supports the fonts and the corners of schemes. See <see cref="BootstrapThemeSchemeTokens"/>.
    /// </summary>
    public ThemeSchemeOptions UseBootstrap()
    {
        Features |= ThemeSchemeFeatures.Fonts | ThemeSchemeFeatures.Corners;

        return AddTokens<BootstrapThemeSchemeTokens>();
    }

    /// <summary>
    /// The neutral blue scheme seeded for a theme that names no default schemes.
    /// </summary>
    public static ThemeScheme CreateNeutralScheme()
        => new()
        {
            Id = NeutralSchemeId,
            Name = "Default",
            Position = 0,
            Primary = "#2563eb",
            Deep = "#1e3a8a",
            Ink = "#0f172a",
            Accent = "#60a5fa",
        };

    /// <summary>
    /// The default token prefix of a theme: its id in lowercase, with every character other than a letter or a digit
    /// turned into a dash.
    /// </summary>
    public static string CreateTokenPrefix(string themeId)
    {
        if (string.IsNullOrEmpty(themeId))
        {
            return "theme";
        }

        var prefix = new StringBuilder(themeId.Length);

        foreach (var c in themeId)
        {
            prefix.Append(char.IsAsciiLetterOrDigit(c) ? char.ToLowerInvariant(c) : '-');
        }

        return prefix.ToString();
    }

    // The schemes to seed: the theme's own, or the neutral one.
    internal IEnumerable<ThemeScheme> GetSchemesToSeed()
        => Defaults is { Count: > 0 } ? Defaults : [CreateNeutralScheme()];

    // The scheme the theme uses when it has no stored scheme at all, for example when its schemes were removed outside
    // the admin: its default scheme among the ones it seeds, or the first of them. It is never stored.
    internal ThemeScheme GetFallbackScheme()
    {
        var seeds = GetSchemesToSeed().ToList();

        return seeds.FirstOrDefault(scheme => string.Equals(scheme.Id, DefaultSchemeId, StringComparison.Ordinal)) ?? seeds[0];
    }
}
