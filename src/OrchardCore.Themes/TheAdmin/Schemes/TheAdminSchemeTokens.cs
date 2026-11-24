using System.Text;
using OrchardCore.Themes.TheAdmin.Models;

namespace OrchardCore.Themes.TheAdmin.Schemes;

/// <summary>
/// The tokens only TheAdmin's layout uses, added to the generated ones: the menu (<c>--ta-menu-*</c>), the toolbar of
/// lists (<c>--ta-toolbar-*</c>), the header of lists, the top bar, the page gradient and tint, and the icons of
/// check boxes, radio buttons and switches drawn in the color of text on the primary; and the colors of the swatch of
/// each scheme in the pickers. With the generated tokens, they are the C# counterpart of the scheme mixins of the
/// theme's Sass, which build the stylesheet's fallback scheme; a parity test keeps both in step.
/// </summary>
public static class TheAdminSchemeTokens
{
    private static readonly AdminColor s_lightMenu = AdminColor.Parse("#f1f3f5");
    private static readonly AdminColor s_lightMenuText = AdminColor.Parse("#495057");
    private static readonly AdminColor s_lightMenuBorder = AdminColor.Parse("#dee2e6");

    /// <summary>
    /// Writes the layout's tokens of a scheme for one mode, after the generated ones.
    /// </summary>
    public static void WriteTokens(AdminSchemePalette palette, AdminSchemeMode mode, AdminSchemeTokenWriter tokens)
    {
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(tokens);

        if (mode == AdminSchemeMode.Dark)
        {
            WriteDark(palette, tokens);
        }
        else
        {
            WriteLight(palette, tokens);
        }
    }

    /// <summary>
    /// Appends the rules of the swatch of a scheme in the pickers, drawn whatever the scheme of the page.
    /// </summary>
    public static void AppendRules(StringBuilder css, AdminScheme scheme, AdminSchemePalette palette)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(scheme);
        ArgumentNullException.ThrowIfNull(palette);

        // The swatch shows the menu first: its dark end and its top, or the light gray of a light menu.
        var swatch = palette.LightMenu
            ? new[] { s_lightMenu, s_lightMenuBorder, palette.Primary, palette.Deep }
            : new[] { palette.Ink, palette.Deep, palette.Primary, palette.Accent };

        for (var i = 0; i < swatch.Length; i++)
        {
            css.Append(".ta-scheme-swatch[data-scheme=\"").Append(scheme.Id).Append("\"]>span:nth-child(").Append(i + 1)
                .Append("){background-color:").Append(swatch[i].ToHex()).Append("}\n");
        }
    }

    private static void WriteLight(AdminSchemePalette palette, AdminSchemeTokenWriter tokens)
    {
        var colors = palette.Light;
        var primary = colors.Primary;
        var on = palette.OnPrimary;
        var toolbar = (palette.PickedToolbar ?? primary).EnsureContrast([on], AdminSchemePalette.TextContrast, palette.Ink);
        var k = palette.Saturation;

        tokens.Add("tint", primary.Mix(AdminColor.White, 0.09))
            .Add("toolbar-bg", toolbar)
            .AddWithRgb("toolbar-fg", on)
            .Add("toolbar-hover-bg", palette.HasLightTextOnPrimary ? AdminColor.Black.ToRgba(0.16) : AdminColor.White.ToRgba(0.3))
            .Add("list-header-bg", colors.HeaderSurface)
            .Add("topbar-color", AdminColor.FromHsl(palette.Hue, 0.28 * k, 0.28))
            .Add("page-gradient", Gradient(135, (colors.PageTintStart, 0), (colors.Page, 48), (colors.PageTintEnd, 100)));

        if (palette.LightMenu)
        {
            WriteLightMenu(tokens, primary);
        }
        else
        {
            var menuMiddle = palette.Deep.Mix(palette.Ink, 0.5);

            WriteDarkMenu(tokens, palette, primary);
            tokens.Add("menu-bg", menuMiddle)
                .Add("menu-gradient", Gradient(180, (palette.Deep, 0), (menuMiddle, 48), (palette.Ink, 100)));
        }

        WriteControlIcons(tokens, on);
    }

    private static void WriteDark(AdminSchemePalette palette, AdminSchemeTokenWriter tokens)
    {
        var colors = palette.Dark;
        var hue = palette.Hue;
        var k = palette.Saturation;

        tokens.Add("list-header-bg", colors.HeaderSurface)
            .Add("topbar-bg", AdminColor.FromHsl(hue, 0.56 * k, 0.17))
            .Add("topbar-color", AdminColor.FromHsl(hue, 0.30 * k, 0.83))
            .Add("menu-bg", AdminColor.FromHsl(hue, 0.70 * k, 0.17))
            .Add("menu-gradient", Gradient(
                180,
                (AdminColor.FromHsl(hue, 0.68 * k, 0.235), 0),
                (AdminColor.FromHsl(hue, 0.70 * k, 0.18), 55),
                (AdminColor.FromHsl(hue, 0.71 * k, 0.147), 100)))
            .Add("page-gradient", Gradient(135, (colors.PageTintStart, 0), (colors.Page, 48), (colors.PageTintEnd, 100)));

        WriteDarkMenu(tokens, palette, colors.Primary);
    }

    private static void WriteDarkMenu(AdminSchemeTokenWriter tokens, AdminSchemePalette palette, AdminColor primary)
        => tokens.Add("menu-strong", "#fff")
            .Add("menu-strong-rgb", "255, 255, 255")
            .Add("menu-color", AdminColor.White.ToRgba(0.78))
            .Add("menu-muted", AdminColor.White.ToRgba(0.72))
            .Add("menu-hover-bg", AdminColor.White.ToRgba(0.08))
            .Add("menu-border", AdminColor.White.ToRgba(0.08))
            .Add("menu-active-bg", primary)
            .Add("menu-active-color", palette.OnPrimary)
            .AddWithRgb("menu-accent", palette.Accent);

    private static void WriteLightMenu(AdminSchemeTokenWriter tokens, AdminColor primary)
        => tokens.Add("menu-strong", "#212529")
            .Add("menu-strong-rgb", "33, 37, 41")
            .Add("menu-color", "#495057")
            .Add("menu-muted", "#5c636a")
            .Add("menu-hover-bg", s_lightMenuText.ToRgba(0.08))
            .Add("menu-border", "#dee2e6")
            .Add("menu-active-bg", s_lightMenuText.Mix(s_lightMenu, 0.14))
            .Add("menu-active-color", "#212529")
            .AddWithRgb("menu-accent", primary.EnsureContrast([s_lightMenu], AdminSchemePalette.UiContrast, AdminColor.Black))
            .Add("menu-bg", s_lightMenu)
            .Add("menu-gradient", Gradient(180, (s_lightMenu, 0), (s_lightMenu, 100)));

    // The marks of checked controls, drawn in the color of text on the primary, which fills them.
    private static void WriteControlIcons(AdminSchemeTokenWriter tokens, AdminColor on)
    {
        var c = "%23" + on.ToHex()[1..];

        tokens.Add("check-icon", $"url(\"data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 20 20'%3e%3cpath fill='none' stroke='{c}' stroke-linecap='round' stroke-linejoin='round' stroke-width='3' d='m6 10 3 3 6-6'/%3e%3c/svg%3e\")")
            .Add("indeterminate-icon", $"url(\"data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 20 20'%3e%3cpath fill='none' stroke='{c}' stroke-linecap='round' stroke-linejoin='round' stroke-width='3' d='M6 10h8'/%3e%3c/svg%3e\")")
            .Add("radio-icon", $"url(\"data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='-4 -4 8 8'%3e%3ccircle r='2' fill='{c}'/%3e%3c/svg%3e\")")
            .Add("switch-icon", $"url(\"data:image/svg+xml,%3csvg xmlns='http://www.w3.org/2000/svg' viewBox='-4 -4 8 8'%3e%3ccircle r='3' fill='{c}'/%3e%3c/svg%3e\")");
    }

    private static string Gradient(int angle, params (AdminColor Color, int Stop)[] stops)
        => $"linear-gradient({angle}deg, {string.Join(", ", stops.Select(stop => $"{stop.Color.ToHex()} {stop.Stop}%"))})";
}
