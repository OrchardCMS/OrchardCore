using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes;

/// <summary>
/// A <see cref="ThemeScheme"/> resolved into colors, light and dark, and its typography and shape. What the optional
/// settings of the scheme leave empty is worked out from its four colors or takes its default; text and control
/// boundaries are raised to WCAG 2.2 AA contrast against every surface they sit on, whatever colors an admin picks.
/// It assumes no CSS framework: <see cref="ThemeSchemeStylesheet"/> writes it as tokens, and
/// <see cref="IThemeSchemeTokenContributor"/>s read it to write their own.
/// </summary>
public sealed class ThemeSchemePalette
{
    /// <summary>
    /// The contrast text is raised to: WCAG 2.2 AA's 4.5:1, with a small margin for rounding.
    /// </summary>
    public const double TextContrast = 4.6;

    /// <summary>
    /// The contrast control boundaries and focus rings are raised to: WCAG 2.2 AA's 3:1, with a small margin for
    /// rounding.
    /// </summary>
    public const double UiContrast = 3.1;

    private static readonly ThemeColor s_muted = ThemeColor.Parse("#6d7384");

    /// <summary>
    /// Gets the primary as picked: actions, links and the current page.
    /// </summary>
    public ThemeColor Primary { get; private init; }

    /// <summary>
    /// Gets the deep color: headings and dark areas.
    /// </summary>
    public ThemeColor Deep { get; private init; }

    /// <summary>
    /// Gets the ink: the darkest color.
    /// </summary>
    public ThemeColor Ink { get; private init; }

    /// <summary>
    /// Gets the accent: links and highlights on dark surfaces.
    /// </summary>
    public ThemeColor Accent { get; private init; }

    /// <summary>
    /// Gets the hue of the page, borders and dark mode surfaces, 0 to 360.
    /// </summary>
    public double Hue { get; private init; }

    /// <summary>
    /// Gets how saturated the page, borders and dark mode surfaces are, 0 (gray) to 1.
    /// </summary>
    public double Saturation { get; private init; } = 1;

    /// <summary>
    /// Gets the color of text on the primary: the scheme's, or else white, or dark on a light primary.
    /// </summary>
    public ThemeColor OnPrimary { get; private init; } = ThemeColor.White;

    /// <summary>
    /// Gets whether the scheme asks for a light menu (see <see cref="ThemeScheme.LightMenu"/>).
    /// </summary>
    public bool LightMenu { get; private init; }

    /// <summary>
    /// Gets the page color in light mode the scheme picked, or <see langword="null"/>.
    /// </summary>
    public ThemeColor? PickedPage { get; private init; }

    /// <summary>
    /// Gets the toolbar color the scheme picked, or <see langword="null"/>.
    /// </summary>
    public ThemeColor? PickedToolbar { get; private init; }

    /// <summary>
    /// Gets the page tint in light mode the scheme picked, or <see langword="null"/>.
    /// </summary>
    public ThemeColor? PickedTint { get; private init; }

    /// <summary>
    /// Gets the page tint in dark mode the scheme picked, or <see langword="null"/>.
    /// </summary>
    public ThemeColor? PickedTintDark { get; private init; }

    /// <summary>
    /// Gets the key of the body font in <see cref="ThemeSchemeTypography.Fonts"/>.
    /// </summary>
    public string FontFamily { get; private init; } = ThemeSchemeTypography.DefaultFontFamily;

    /// <summary>
    /// Gets the key of the heading font in <see cref="ThemeSchemeTypography.Fonts"/>, or <see langword="null"/> for
    /// the body font.
    /// </summary>
    public string HeadingFontFamily { get; private init; }

    /// <summary>
    /// Gets the scale of the base font size, clamped to its range.
    /// </summary>
    public double FontScale { get; private init; } = ThemeSchemeTypography.DefaultFontScale;

    /// <summary>
    /// Gets the scale of the corner radii, clamped to its range.
    /// </summary>
    public double RadiusScale { get; private init; } = ThemeSchemeTypography.DefaultRadiusScale;

    /// <summary>
    /// Gets the scale of the spacing of controls and lists, clamped to its range.
    /// </summary>
    public double Density { get; private init; } = ThemeSchemeTypography.DefaultDensity;

    /// <summary>
    /// Gets the colors of the light mode.
    /// </summary>
    public ThemeSchemeColors Light { get; private set; }

    /// <summary>
    /// Gets the colors of the dark mode.
    /// </summary>
    public ThemeSchemeColors Dark { get; private set; }

    /// <summary>
    /// Gets whether text on the primary is light, so hovering and pressing darken the primary rather than lighten it.
    /// </summary>
    public bool HasLightTextOnPrimary
        => OnPrimary.Luminance > 0.5;

    /// <summary>
    /// The colors of the given mode.
    /// </summary>
    public ThemeSchemeColors Get(ThemeSchemeMode mode)
        => mode == ThemeSchemeMode.Dark ? Dark : Light;

    /// <summary>
    /// Resolves a scheme, or returns <see langword="null"/> when one of its colors is not valid. An unknown font falls
    /// back on the default one and a scale out of its range is clamped.
    /// </summary>
    public static ThemeSchemePalette FromScheme(ThemeScheme scheme)
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

        var (hue, saturation) = DeriveHueSaturation(deep);

        return new ThemeSchemePalette
        {
            Primary = primary,
            Deep = deep,
            Ink = ink,
            Accent = accent,
            LightMenu = scheme.LightMenu,
            Hue = scheme.Hue is { } pickedHue && double.IsFinite(pickedHue) ? ((pickedHue % 360) + 360) % 360 : hue,
            Saturation = scheme.Saturation is { } pickedSaturation && double.IsFinite(pickedSaturation) ? Math.Clamp(pickedSaturation, 0, 1) : saturation,
            OnPrimary = onPrimary ?? DeriveOnPrimary(primary, ink) ?? ThemeColor.White,
            PickedPage = page,
            PickedToolbar = toolbar,
            PickedTint = tint,
            PickedTintDark = tintDark,
            FontFamily = ThemeSchemeTypography.FindFont(scheme.FontFamily)?.Key ?? ThemeSchemeTypography.DefaultFontFamily,
            HeadingFontFamily = ThemeSchemeTypography.FindFont(scheme.HeadingFontFamily)?.Key,
            FontScale = Clamp(scheme.FontScale, ThemeSchemeTypography.MinFontScale, ThemeSchemeTypography.MaxFontScale, ThemeSchemeTypography.DefaultFontScale),
            RadiusScale = Clamp(scheme.RadiusScale, ThemeSchemeTypography.MinRadiusScale, ThemeSchemeTypography.MaxRadiusScale, ThemeSchemeTypography.DefaultRadiusScale),
            Density = Clamp(scheme.Density, ThemeSchemeTypography.MinDensity, ThemeSchemeTypography.MaxDensity, ThemeSchemeTypography.DefaultDensity),
        }.Derive();
    }

    /// <summary>
    /// Resolves a scheme of four colors only, working out the rest: the hue and saturation from the deep color, and
    /// dark text on the primary when white text would need the primary darkened too far (a light primary such as a
    /// gold).
    /// </summary>
    public static ThemeSchemePalette FromColors(ThemeColor primary, ThemeColor deep, ThemeColor ink, ThemeColor accent, bool lightMenu = false)
    {
        var (hue, saturation) = DeriveHueSaturation(deep);

        return new ThemeSchemePalette
        {
            Primary = primary,
            Deep = deep,
            Ink = ink,
            Accent = accent,
            LightMenu = lightMenu,
            Hue = hue,
            Saturation = saturation,
            OnPrimary = DeriveOnPrimary(primary, ink) ?? ThemeColor.White,
        }.Derive();
    }

    /// <summary>
    /// Moves the primary away from its text color, to hover or press it: toward <paramref name="towardDark"/> under
    /// light text, toward white under dark text.
    /// </summary>
    public ThemeColor Press(ThemeColor primary, ThemeColor towardDark, double amount)
        => HasLightTextOnPrimary ? towardDark.Mix(primary, amount) : ThemeColor.White.Mix(primary, amount);

    // The hue of the deep color, and its saturation stretched so that the built-in schemes keep the tones they were
    // tuned with.
    private static (double Hue, double Saturation) DeriveHueSaturation(ThemeColor deep)
    {
        var (hue, saturation) = deep.ToHueSaturation();

        return (hue, Math.Clamp(saturation / 0.7, 0.08, 1));
    }

    // White text that would only pass on a much darker primary: the primary keeps its color, the text turns dark.
    private static ThemeColor? DeriveOnPrimary(ThemeColor primary, ThemeColor ink)
    {
        if (primary.ContrastWith(ThemeColor.White) >= 3)
        {
            return null;
        }

        return ink.ContrastWith(primary) >= TextContrast ? ink : new ThemeColor(17, 17, 17);
    }

    private ThemeSchemePalette Derive()
    {
        var on = OnPrimary;
        var primary = Primary.EnsureContrast([on], TextContrast, Ink);

        Light = DeriveLight(primary);
        Dark = DeriveDark(primary);

        return this;
    }

    private ThemeSchemeColors DeriveLight(ThemeColor primary)
    {
        var k = Saturation;
        var page = PickedPage ?? ThemeColor.FromHsl(Hue, 0.5 * k, 0.97);
        var surfaceMuted = ThemeColor.FromHsl(Hue, 0.6 * k, 0.982);
        var tintStart = (PickedTint ?? primary).Mix(page, 0.07);
        var tintEnd = (PickedTint ?? Accent).Mix(page, 0.09);
        var header = ThemeColor.FromHsl(Hue, 0.14 * k, 0.975);
        var rowHover = primary.Mix(ThemeColor.White, 0.08);
        ThemeColor[] surfaces = [page, ThemeColor.White, surfaceMuted, tintStart, tintEnd, header, rowHover];
        var border = ThemeColor.FromHsl(Hue, 0.38 * k, 0.92);
        var link = primary.EnsureContrast(surfaces, TextContrast, Ink);

        return new ThemeSchemeColors
        {
            Primary = primary,
            PrimaryHover = Press(primary, Deep, 0.22),
            PrimaryActive = Press(primary, Deep, 0.34),
            PrimarySubtle = primary.Mix(ThemeColor.White, 0.2),
            PrimaryBorderSubtle = primary.Mix(ThemeColor.White, 0.4),
            PrimaryEmphasis = primary.Mix(ThemeColor.Black, 0.4),
            Link = link,
            LinkHover = Deep,
            Focus = link,
            Page = page,
            Surface = ThemeColor.White,
            SurfaceMuted = surfaceMuted,
            Heading = Deep,
            Label = ThemeColor.FromHsl(Hue, 0.32 * k, 0.35),
            Muted = s_muted.EnsureContrast(surfaces, TextContrast, ThemeColor.Black),
            Border = border,
            ControlBorder = border.EnsureContrast(surfaces, UiContrast, Deep),
            ControlCheckedBorder = primary.EnsureContrast(surfaces, UiContrast, Ink),
            RowHover = rowHover,
            PageTintStart = tintStart,
            PageTintEnd = tintEnd,
            HeaderSurface = header,
            Surfaces = surfaces,
        };
    }

    private ThemeSchemeColors DeriveDark(ThemeColor primary)
    {
        var k = Saturation;
        var page = ThemeColor.FromHsl(Hue, 0.57 * k, 0.135);
        var surface = ThemeColor.FromHsl(Hue, 0.57 * k, 0.185);
        var surfaceMuted = ThemeColor.FromHsl(Hue, 0.56 * k, 0.216);
        var tintStart = (PickedTintDark ?? Accent).Mix(page, 0.08);
        var tintEnd = (PickedTintDark ?? primary).Mix(page, 0.12);
        var header = ThemeColor.FromHsl(Hue, 0.5 * k, 0.21);
        var rowHover = primary.Mix(surface, 0.16);
        ThemeColor[] surfaces = [page, surface, surfaceMuted, tintStart, tintEnd, header, rowHover];
        var border = ThemeColor.FromHsl(Hue, 0.49 * k, 0.28);
        var link = Accent.EnsureContrast(surfaces, TextContrast, ThemeColor.White);
        var subtleBase = ThemeColor.FromHsl(Hue, 0.57 * k, 0.12);

        return new ThemeSchemeColors
        {
            Primary = primary,
            PrimaryHover = Press(primary, Ink, 0.14),
            PrimaryActive = Press(primary, Ink, 0.24),
            PrimarySubtle = primary.Mix(subtleBase, 0.22),
            PrimaryBorderSubtle = primary.Mix(subtleBase, 0.5),
            PrimaryEmphasis = Accent.Mix(ThemeColor.White, 0.6),
            Link = link,
            LinkHover = link.Mix(ThemeColor.White, 0.7),
            Focus = link,
            Page = page,
            Surface = surface,
            SurfaceMuted = surfaceMuted,
            SecondaryBackground = ThemeColor.FromHsl(Hue, 0.55 * k, 0.233),
            BodyColor = ThemeColor.FromHsl(Hue, 0.35 * k, 0.92),
            Heading = ThemeColor.FromHsl(Hue, 0.45 * k, 0.94),
            Label = ThemeColor.FromHsl(Hue, 0.33 * k, 0.79),
            Muted = ThemeColor.FromHsl(Hue, 0.29 * k, 0.70).EnsureContrast(surfaces, TextContrast, ThemeColor.White),
            Border = border,
            ControlBorder = border.EnsureContrast(surfaces, UiContrast, ThemeColor.White),
            ControlCheckedBorder = Accent.EnsureContrast(surfaces, UiContrast, ThemeColor.White),
            RowHover = rowHover,
            PageTintStart = tintStart,
            PageTintEnd = tintEnd,
            HeaderSurface = header,
            Surfaces = surfaces,
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
