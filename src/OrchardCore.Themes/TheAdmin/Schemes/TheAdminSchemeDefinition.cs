using OrchardCore.Themes.Schemes;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.TheAdmin.Schemes;

/// <summary>
/// What a scheme of TheAdmin is made of, worked out from a <see cref="ThemeScheme"/>: four colors and a hue, as in the schemes map of the theme's Sass, and its typography and
/// shape.
/// <list type="bullet">
/// <item><see cref="Primary"/>: actions, links and the current page;</item>
/// <item><see cref="Deep"/>: headings and the top of the menu;</item>
/// <item><see cref="Ink"/>: the bottom of the menu, the darkest surface;</item>
/// <item><see cref="Accent"/>: links and highlights on dark surfaces;</item>
/// <item><see cref="Hue"/> and <see cref="Saturation"/>: the tint of the page, the borders and the dark mode surfaces;</item>
/// <item><see cref="FontFamily"/>, <see cref="HeadingFontFamily"/>, <see cref="FontScale"/>, <see cref="RadiusScale"/>
/// and <see cref="Density"/>: the type, the corners and the spacing, the same in both modes.</item>
/// </list>
/// </summary>
public sealed class TheAdminSchemeDefinition
{
    public ThemeColor Primary { get; init; }

    public ThemeColor Deep { get; init; }

    public ThemeColor Ink { get; init; }

    public ThemeColor Accent { get; init; }

    /// <summary>
    /// The hue of the scheme's surfaces, 0 to 360.
    /// </summary>
    public double Hue { get; init; }

    /// <summary>
    /// How saturated the scheme's surfaces are, 0 (gray) to 1.
    /// </summary>
    public double Saturation { get; init; } = 1;

    /// <summary>
    /// Whether the menu is light gray with dark text, as in the former TheAdmin, rather than dark on the scheme's colors.
    /// </summary>
    public bool LightMenu { get; init; }

    /// <summary>
    /// The color of text and marks drawn on the primary; white when not set.
    /// </summary>
    public ThemeColor? OnPrimary { get; init; }

    /// <summary>
    /// The page color in light mode; a pale tone of the hue when not set.
    /// </summary>
    public ThemeColor? Page { get; init; }

    /// <summary>
    /// The color of both ends of the page gradient in light mode; the primary and the accent when not set.
    /// </summary>
    public ThemeColor? Tint { get; init; }

    /// <summary>
    /// The color of both ends of the page gradient in dark mode; the accent and the primary when not set.
    /// </summary>
    public ThemeColor? TintDark { get; init; }

    /// <summary>
    /// The color of the toolbar of lists; the primary when not set.
    /// </summary>
    public ThemeColor? Toolbar { get; init; }

    /// <summary>
    /// The body font, one of the keys of <see cref="ThemeSchemeTypography.Fonts"/>.
    /// </summary>
    public string FontFamily { get; init; } = ThemeSchemeTypography.DefaultFontFamily;

    /// <summary>
    /// The font of headings, one of the keys of <see cref="ThemeSchemeTypography.Fonts"/>; the body font when
    /// <see langword="null"/>.
    /// </summary>
    public string HeadingFontFamily { get; init; }

    /// <summary>
    /// The scale of the base font size, from <see cref="ThemeSchemeTypography.MinFontScale"/> to
    /// <see cref="ThemeSchemeTypography.MaxFontScale"/>.
    /// </summary>
    public double FontScale { get; init; } = ThemeSchemeTypography.DefaultFontScale;

    /// <summary>
    /// The scale of the corner radii, from <see cref="ThemeSchemeTypography.MinRadiusScale"/> (square) to
    /// <see cref="ThemeSchemeTypography.MaxRadiusScale"/>.
    /// </summary>
    public double RadiusScale { get; init; } = ThemeSchemeTypography.DefaultRadiusScale;

    /// <summary>
    /// The scale of the spacing of controls and lists, from <see cref="ThemeSchemeTypography.MinDensity"/> (compact)
    /// to <see cref="ThemeSchemeTypography.MaxDensity"/>.
    /// </summary>
    public double Density { get; init; } = ThemeSchemeTypography.DefaultDensity;

    /// <summary>
    /// Builds the definition of a scheme from its four colors only, working out the rest the way the built-in schemes
    /// were tuned: the hue and saturation from the deep color, and dark text on the primary when white text would need
    /// the primary darkened too far (a light primary such as a gold).
    /// </summary>
    public static TheAdminSchemeDefinition FromColors(ThemeColor primary, ThemeColor deep, ThemeColor ink, ThemeColor accent, bool lightMenu)
    {
        var (hue, saturation) = deep.ToHueSaturation();
        ThemeColor? onPrimary = null;

        // White text that would only pass on a much darker primary: the primary keeps its color, the text turns dark.
        if (primary.ContrastWith(ThemeColor.White) < 3)
        {
            var dark = ink.ContrastWith(primary) >= TheAdminSchemeCssBuilder.TextContrast ? ink : new ThemeColor(17, 17, 17);
            onPrimary = dark;
        }

        return new TheAdminSchemeDefinition
        {
            Primary = primary,
            Deep = deep,
            Ink = ink,
            Accent = accent,
            Hue = hue,
            Saturation = Math.Clamp(saturation / 0.7, 0.08, 1),
            LightMenu = lightMenu,
            OnPrimary = onPrimary,
        };
    }

    /// <summary>
    /// The definition the tokens of a scheme are derived from, or <see langword="null"/> when a color is not valid.
    /// What the optional settings leave empty is worked out from the four colors or takes its default; an unknown font
    /// falls back on the default and a scale out of its range is clamped.
    /// </summary>
    public static TheAdminSchemeDefinition FromScheme(ThemeScheme scheme)
    {
        ArgumentNullException.ThrowIfNull(scheme);

        if (!ThemeColor.TryParse(scheme.Primary, out var primary)
            || !ThemeColor.TryParse(scheme.Deep, out var deep)
            || !ThemeColor.TryParse(scheme.Ink, out var ink)
            || !ThemeColor.TryParse(scheme.Accent, out var accent)
            || !TryParseOptional(scheme.OnPrimary, out var onPrimary)
            || !TryParseOptional(scheme.Page, out var page)
            || !TryParseOptional(scheme.Toolbar, out var toolbar)
            || !TryParseOptional(scheme.Tint, out var tint)
            || !TryParseOptional(scheme.TintDark, out var tintDark))
        {
            return null;
        }

        var derived = FromColors(primary, deep, ink, accent, scheme.LightMenu);

        return new TheAdminSchemeDefinition
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
            FontFamily = ThemeSchemeTypography.FindFont(scheme.FontFamily)?.Key ?? ThemeSchemeTypography.DefaultFontFamily,
            HeadingFontFamily = ThemeSchemeTypography.FindFont(scheme.HeadingFontFamily)?.Key,
            FontScale = Clamp(scheme.FontScale, ThemeSchemeTypography.MinFontScale, ThemeSchemeTypography.MaxFontScale, ThemeSchemeTypography.DefaultFontScale),
            RadiusScale = Clamp(scheme.RadiusScale, ThemeSchemeTypography.MinRadiusScale, ThemeSchemeTypography.MaxRadiusScale, ThemeSchemeTypography.DefaultRadiusScale),
            Density = Clamp(scheme.Density, ThemeSchemeTypography.MinDensity, ThemeSchemeTypography.MaxDensity, ThemeSchemeTypography.DefaultDensity),
        };
    }

    private static double Clamp(double? value, double min, double max, double fallback)
        => value is { } number && double.IsFinite(number) ? Math.Clamp(number, min, max) : fallback;

    private static bool TryParseOptional(string value, out ThemeColor? color)
    {
        color = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (!ThemeColor.TryParse(value, out var parsed))
        {
            return false;
        }

        color = parsed;

        return true;
    }
}
