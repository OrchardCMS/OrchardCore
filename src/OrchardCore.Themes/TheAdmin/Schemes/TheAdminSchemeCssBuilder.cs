using System.Text;
using OrchardCore.Themes.Schemes;

namespace OrchardCore.Themes.TheAdmin.Schemes;

/// <summary>
/// Writes the CSS of a scheme of TheAdmin: its '--ta-*' color tokens for the light and the dark mode, its typography and shape
/// tokens (the same in both modes, so written once, with the light ones), and the colors of its swatch. The color
/// tokens are the C# counterpart of the scheme mixins of the theme's Sass, which build the stylesheet's fallback
/// scheme; a parity test keeps both in step.
/// </summary>
public static class TheAdminSchemeCssBuilder
{
    // WCAG 2.2 AA with a small margin for rounding: 4.5:1 for text, 3:1 for the boundaries of controls and focus rings.
    public const double TextContrast = 4.6;

    public const double UiContrast = 3.1;

    private static readonly ThemeColor s_muted = ThemeColor.Parse("#6d7384");
    private static readonly ThemeColor s_lightMenu = ThemeColor.Parse("#f1f3f5");
    private static readonly ThemeColor s_lightMenuText = ThemeColor.Parse("#495057");

    /// <summary>
    /// Appends the rules of the scheme <paramref name="id"/> to <paramref name="css"/>. The id must be a CSS identifier
    /// (letters, digits and dashes); the caller validates it.
    /// </summary>
    public static void Append(StringBuilder css, string id, TheAdminSchemeDefinition scheme)
    {
        css.Append("[data-ta-scheme=\"").Append(id).Append("\"]{");
        AppendTokens(css, BuildLight(scheme));
        AppendTokens(css, BuildTypography(scheme));
        css.Append("}\n");

        css.Append("[data-bs-theme=\"dark\"][data-ta-scheme=\"").Append(id).Append("\"]{");
        AppendTokens(css, BuildDark(scheme));
        css.Append("}\n");

        // The swatch shows the menu first: its dark end and its top, or the light gray of a light menu.
        var swatch = scheme.LightMenu
            ? new[] { s_lightMenu, ThemeColor.Parse("#dee2e6"), scheme.Primary, scheme.Deep }
            : new[] { scheme.Ink, scheme.Deep, scheme.Primary, scheme.Accent };

        for (var i = 0; i < swatch.Length; i++)
        {
            css.Append(".ta-scheme-swatch[data-scheme=\"").Append(id).Append("\"]>span:nth-child(").Append(i + 1)
                .Append("){background-color:").Append(swatch[i].ToHex()).Append("}\n");
        }
    }

    /// <summary>
    /// The tokens of the light mode, and the ones both modes share, in the order the Sass mixin writes them.
    /// </summary>
    public static IReadOnlyList<KeyValuePair<string, string>> BuildLight(TheAdminSchemeDefinition scheme)
    {
        var tokens = new List<KeyValuePair<string, string>>();
        var on = scheme.OnPrimary ?? ThemeColor.White;
        var deep = scheme.Deep;
        var ink = scheme.Ink;
        var primary = scheme.Primary.EnsureContrast([on], TextContrast, ink);
        var accent = scheme.Accent;
        var hue = scheme.Hue;
        var k = scheme.Saturation;
        var page = scheme.Page ?? ThemeColor.FromHsl(hue, 0.5 * k, 0.97);
        var surfaceMuted = ThemeColor.FromHsl(hue, 0.6 * k, 0.982);
        var tintStart = (scheme.Tint ?? primary).Mix(page, 0.07);
        var tintEnd = (scheme.Tint ?? accent).Mix(page, 0.09);
        var toolbar = (scheme.Toolbar ?? primary).EnsureContrast([on], TextContrast, ink);
        var listHeader = ThemeColor.FromHsl(hue, 0.14 * k, 0.975);
        var rowHover = primary.Mix(ThemeColor.White, 0.08);
        ThemeColor[] surfaces = [page, ThemeColor.White, surfaceMuted, tintStart, tintEnd, listHeader, rowHover];
        var border = ThemeColor.FromHsl(hue, 0.38 * k, 0.92);
        var menuMiddle = deep.Mix(ink, 0.5);
        var link = primary.EnsureContrast(surfaces, TextContrast, ink);
        var lightText = on.Luminance > 0.5;

        Add(tokens, "--ta-primary", primary);
        tokens.Add(new("--ta-primary-rgb", primary.ToTriplet()));
        Add(tokens, "--ta-primary-hover", Press(primary, lightText, deep, 0.22));
        Add(tokens, "--ta-primary-active", Press(primary, lightText, deep, 0.34));
        Add(tokens, "--ta-primary-subtle", primary.Mix(ThemeColor.White, 0.2));
        Add(tokens, "--ta-primary-border-subtle", primary.Mix(ThemeColor.White, 0.4));
        Add(tokens, "--ta-primary-emphasis", primary.Mix(ThemeColor.Black, 0.4));
        Add(tokens, "--ta-navy", deep);
        tokens.Add(new("--ta-navy-rgb", deep.ToTriplet()));
        Add(tokens, "--ta-ink", ink);
        Add(tokens, "--ta-sky", accent);
        tokens.Add(new("--ta-sky-rgb", accent.ToTriplet()));
        Add(tokens, "--ta-tint", primary.Mix(ThemeColor.White, 0.09));
        Add(tokens, "--ta-link", link);
        tokens.Add(new("--ta-link-rgb", link.ToTriplet()));
        Add(tokens, "--ta-focus-color", link);
        Add(tokens, "--ta-link-hover", deep);
        tokens.Add(new("--ta-link-hover-rgb", deep.ToTriplet()));
        Add(tokens, "--ta-page-bg", page);
        tokens.Add(new("--ta-surface", "#ffffff"));
        tokens.Add(new("--ta-surface-rgb", "255, 255, 255"));
        Add(tokens, "--ta-surface-muted", surfaceMuted);
        Add(tokens, "--ta-toolbar-bg", toolbar);
        Add(tokens, "--ta-toolbar-fg", on);
        tokens.Add(new("--ta-toolbar-fg-rgb", on.ToTriplet()));
        tokens.Add(new("--ta-toolbar-hover-bg", lightText ? ThemeColor.Black.ToRgba(0.16) : ThemeColor.White.ToRgba(0.3)));
        Add(tokens, "--ta-list-header-bg", listHeader);
        Add(tokens, "--ta-row-hover-bg", rowHover);
        Add(tokens, "--ta-border", border);
        Add(tokens, "--ta-control-border", border.EnsureContrast(surfaces, UiContrast, deep));
        Add(tokens, "--ta-control-checked-border", primary.EnsureContrast(surfaces, UiContrast, ink));
        AddOnPrimaryTokens(tokens, on);
        Add(tokens, "--ta-muted", s_muted.EnsureContrast(surfaces, TextContrast, ThemeColor.Black));
        Add(tokens, "--ta-heading", deep);
        Add(tokens, "--ta-label", ThemeColor.FromHsl(hue, 0.32 * k, 0.35));
        Add(tokens, "--ta-topbar-color", ThemeColor.FromHsl(hue, 0.28 * k, 0.28));
        tokens.Add(new("--ta-page-gradient", Gradient(135, (tintStart, 0), (page, 48), (tintEnd, 100))));

        if (scheme.LightMenu)
        {
            AddLightMenu(tokens, primary);
        }
        else
        {
            AddDarkMenu(tokens, scheme, primary, on);
            Add(tokens, "--ta-menu-bg", menuMiddle);
            tokens.Add(new("--ta-menu-gradient", Gradient(180, (deep, 0), (menuMiddle, 48), (ink, 100))));
        }

        return tokens;
    }

    /// <summary>
    /// The typography and shape tokens: the body and heading fonts, and the font, radius and density scales. An
    /// unknown font falls back on the default one and a scale out of its range is clamped, so the stylesheet always
    /// holds valid values.
    /// </summary>
    public static IReadOnlyList<KeyValuePair<string, string>> BuildTypography(TheAdminSchemeDefinition scheme)
    {
        var body = ThemeSchemeTypography.FindFont(scheme.FontFamily)
            ?? ThemeSchemeTypography.FindFont(ThemeSchemeTypography.DefaultFontFamily);
        var heading = ThemeSchemeTypography.FindFont(scheme.HeadingFontFamily);

        return
        [
            new("--ta-font-family", body.Stack),
            new("--ta-heading-font-family", heading?.Stack ?? "var(--ta-font-family)"),
            new("--ta-font-scale", Scale(scheme.FontScale, ThemeSchemeTypography.MinFontScale, ThemeSchemeTypography.MaxFontScale, ThemeSchemeTypography.DefaultFontScale)),
            new("--ta-radius-scale", Scale(scheme.RadiusScale, ThemeSchemeTypography.MinRadiusScale, ThemeSchemeTypography.MaxRadiusScale, ThemeSchemeTypography.DefaultRadiusScale)),
            new("--ta-density", Scale(scheme.Density, ThemeSchemeTypography.MinDensity, ThemeSchemeTypography.MaxDensity, ThemeSchemeTypography.DefaultDensity)),
        ];
    }

    /// <summary>
    /// The tokens of the dark mode, in the order the Sass mixin writes them.
    /// </summary>
    public static IReadOnlyList<KeyValuePair<string, string>> BuildDark(TheAdminSchemeDefinition scheme)
    {
        var tokens = new List<KeyValuePair<string, string>>();
        var on = scheme.OnPrimary ?? ThemeColor.White;
        var ink = scheme.Ink;
        var primary = scheme.Primary.EnsureContrast([on], TextContrast, ink);
        var accent = scheme.Accent;
        var hue = scheme.Hue;
        var k = scheme.Saturation;
        var page = ThemeColor.FromHsl(hue, 0.57 * k, 0.135);
        var surface = ThemeColor.FromHsl(hue, 0.57 * k, 0.185);
        var surfaceMuted = ThemeColor.FromHsl(hue, 0.56 * k, 0.216);
        var tintStart = (scheme.TintDark ?? accent).Mix(page, 0.08);
        var tintEnd = (scheme.TintDark ?? primary).Mix(page, 0.12);
        var listHeader = ThemeColor.FromHsl(hue, 0.5 * k, 0.21);
        var rowHover = primary.Mix(surface, 0.16);
        ThemeColor[] surfaces = [page, surface, surfaceMuted, tintStart, tintEnd, listHeader, rowHover];
        var border = ThemeColor.FromHsl(hue, 0.49 * k, 0.28);
        var bodyColor = ThemeColor.FromHsl(hue, 0.35 * k, 0.92);
        var link = accent.EnsureContrast(surfaces, TextContrast, ThemeColor.White);
        var lightText = on.Luminance > 0.5;
        var subtleBase = ThemeColor.FromHsl(hue, 0.57 * k, 0.12);

        Add(tokens, "--ta-primary-hover", Press(primary, lightText, ink, 0.14));
        Add(tokens, "--ta-primary-active", Press(primary, lightText, ink, 0.24));
        Add(tokens, "--ta-primary-subtle", primary.Mix(subtleBase, 0.22));
        Add(tokens, "--ta-primary-border-subtle", primary.Mix(subtleBase, 0.5));
        Add(tokens, "--ta-primary-emphasis", accent.Mix(ThemeColor.White, 0.6));
        Add(tokens, "--ta-link", link);
        tokens.Add(new("--ta-link-rgb", link.ToTriplet()));
        Add(tokens, "--ta-link-hover", link.Mix(ThemeColor.White, 0.7));
        tokens.Add(new("--ta-link-hover-rgb", link.Mix(ThemeColor.White, 0.7).ToTriplet()));
        Add(tokens, "--ta-focus-color", link);
        Add(tokens, "--ta-heading", ThemeColor.FromHsl(hue, 0.45 * k, 0.94));
        Add(tokens, "--ta-page-bg", page);
        Add(tokens, "--ta-surface", surface);
        tokens.Add(new("--ta-surface-rgb", surface.ToTriplet()));
        Add(tokens, "--ta-surface-muted", surfaceMuted);
        Add(tokens, "--ta-list-header-bg", listHeader);
        Add(tokens, "--ta-row-hover-bg", rowHover);
        Add(tokens, "--ta-secondary-bg", ThemeColor.FromHsl(hue, 0.55 * k, 0.233));
        Add(tokens, "--ta-border", border);
        Add(tokens, "--ta-control-border", border.EnsureContrast(surfaces, UiContrast, ThemeColor.White));
        Add(tokens, "--ta-control-checked-border", accent.EnsureContrast(surfaces, UiContrast, ThemeColor.White));
        Add(tokens, "--ta-muted", ThemeColor.FromHsl(hue, 0.29 * k, 0.70).EnsureContrast(surfaces, TextContrast, ThemeColor.White));
        Add(tokens, "--ta-label", ThemeColor.FromHsl(hue, 0.33 * k, 0.79));
        Add(tokens, "--ta-body-color", bodyColor);
        tokens.Add(new("--ta-body-color-rgb", bodyColor.ToTriplet()));
        Add(tokens, "--ta-topbar-bg", ThemeColor.FromHsl(hue, 0.56 * k, 0.17));
        Add(tokens, "--ta-topbar-color", ThemeColor.FromHsl(hue, 0.30 * k, 0.83));
        Add(tokens, "--ta-menu-bg", ThemeColor.FromHsl(hue, 0.70 * k, 0.17));
        tokens.Add(new("--ta-menu-gradient", Gradient(
            180,
            (ThemeColor.FromHsl(hue, 0.68 * k, 0.235), 0),
            (ThemeColor.FromHsl(hue, 0.70 * k, 0.18), 55),
            (ThemeColor.FromHsl(hue, 0.71 * k, 0.147), 100))));
        tokens.Add(new("--ta-page-gradient", Gradient(135, (tintStart, 0), (page, 48), (tintEnd, 100))));
        AddDarkMenu(tokens, scheme, primary, on);

        return tokens;
    }

    private static void AddDarkMenu(List<KeyValuePair<string, string>> tokens, TheAdminSchemeDefinition scheme, ThemeColor primary, ThemeColor on)
    {
        tokens.Add(new("--ta-menu-strong", "#fff"));
        tokens.Add(new("--ta-menu-strong-rgb", "255, 255, 255"));
        tokens.Add(new("--ta-menu-color", ThemeColor.White.ToRgba(0.78)));
        tokens.Add(new("--ta-menu-muted", ThemeColor.White.ToRgba(0.72)));
        tokens.Add(new("--ta-menu-hover-bg", ThemeColor.White.ToRgba(0.08)));
        tokens.Add(new("--ta-menu-border", ThemeColor.White.ToRgba(0.08)));
        Add(tokens, "--ta-menu-active-bg", primary);
        Add(tokens, "--ta-menu-active-color", on);
        Add(tokens, "--ta-menu-accent", scheme.Accent);
        tokens.Add(new("--ta-menu-accent-rgb", scheme.Accent.ToTriplet()));
    }

    private static void AddLightMenu(List<KeyValuePair<string, string>> tokens, ThemeColor primary)
    {
        var accent = primary.EnsureContrast([s_lightMenu], UiContrast, ThemeColor.Black);

        tokens.Add(new("--ta-menu-strong", "#212529"));
        tokens.Add(new("--ta-menu-strong-rgb", "33, 37, 41"));
        tokens.Add(new("--ta-menu-color", "#495057"));
        tokens.Add(new("--ta-menu-muted", "#5c636a"));
        tokens.Add(new("--ta-menu-hover-bg", s_lightMenuText.ToRgba(0.08)));
        tokens.Add(new("--ta-menu-border", "#dee2e6"));
        Add(tokens, "--ta-menu-active-bg", s_lightMenuText.Mix(s_lightMenu, 0.14));
        tokens.Add(new("--ta-menu-active-color", "#212529"));
        Add(tokens, "--ta-menu-accent", accent);
        tokens.Add(new("--ta-menu-accent-rgb", accent.ToTriplet()));
        Add(tokens, "--ta-menu-bg", s_lightMenu);
        tokens.Add(new("--ta-menu-gradient", Gradient(180, (s_lightMenu, 0), (s_lightMenu, 100))));
    }

    private static void AddOnPrimaryTokens(List<KeyValuePair<string, string>> tokens, ThemeColor on)
    {
        var c = "%23" + on.ToHex()[1..];

        Add(tokens, "--ta-on-primary", on);
        tokens.Add(new("--ta-check-icon", $"url(\"data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 20 20'%3e%3cpath fill='none' stroke='{c}' stroke-linecap='round' stroke-linejoin='round' stroke-width='3' d='m6 10 3 3 6-6'/%3e%3c/svg%3e\")"));
        tokens.Add(new("--ta-indeterminate-icon", $"url(\"data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 20 20'%3e%3cpath fill='none' stroke='{c}' stroke-linecap='round' stroke-linejoin='round' stroke-width='3' d='M6 10h8'/%3e%3c/svg%3e\")"));
        tokens.Add(new("--ta-radio-icon", $"url(\"data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='-4 -4 8 8'%3e%3ccircle r='2' fill='{c}'/%3e%3c/svg%3e\")"));
        tokens.Add(new("--ta-switch-icon", $"url(\"data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='-4 -4 8 8'%3e%3ccircle r='3' fill='{c}'/%3e%3c/svg%3e\")"));
    }

    // Hovering and pressing move the primary away from its text color: darker under light text, lighter under dark.
    private static ThemeColor Press(ThemeColor primary, bool lightText, ThemeColor towardDark, double amount)
        => lightText ? towardDark.Mix(primary, amount) : ThemeColor.White.Mix(primary, amount);

    private static string Gradient(int angle, params (ThemeColor Color, int Stop)[] stops)
        => $"linear-gradient({angle}deg, {string.Join(", ", stops.Select(stop => $"{stop.Color.ToHex()} {stop.Stop}%"))})";

    private static string Scale(double value, double min, double max, double fallback)
        => ThemeSchemeTypography.Format(double.IsFinite(value) ? Math.Clamp(value, min, max) : fallback);

    private static void Add(List<KeyValuePair<string, string>> tokens, string name, ThemeColor color)
        => tokens.Add(new(name, color.ToHex()));

    private static void AppendTokens(StringBuilder css, IReadOnlyList<KeyValuePair<string, string>> tokens)
    {
        foreach (var token in tokens)
        {
            css.Append(token.Key).Append(':').Append(token.Value).Append(';');
        }
    }
}
