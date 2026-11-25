using System.Security.Cryptography;
using System.Text;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Services;

namespace OrchardCore.Themes.TheAdmin.Schemes;

/// <summary>
/// Writes the stylesheet of TheAdmin's schemes. For each scheme with a valid id and valid colors it writes the
/// generated tokens (<see cref="WriteTokens"/>) and the layout's (<see cref="TheAdminSchemeTokens"/>) under
/// <c>[data-ta-scheme="{id}"]</c> for the light mode and under <c>[data-bs-theme="dark"][data-ta-scheme="{id}"]</c> for
/// the dark mode, then the rules of the scheme's swatch.
/// </summary>
public static class AdminSchemeStylesheet
{
    /// <summary>
    /// Builds the stylesheet of the given schemes; the schemes with an invalid id or an invalid color are skipped.
    /// </summary>
    public static string Build(IEnumerable<AdminScheme> schemes)
    {
        ArgumentNullException.ThrowIfNull(schemes);

        var css = new StringBuilder();

        foreach (var scheme in schemes)
        {
            AppendScheme(css, scheme);
        }

        // The rules of a scheme carry Bootstrap's icons (see TheAdminSchemeTokens), so a stylesheet that holds any credits
        // them, as the theme's own stylesheets do.
        return css.Length > 0 ? Banner + css.ToString() : string.Empty;
    }

    /// <summary>
    /// The comment the stylesheet starts with: the icons of checked controls are Bootstrap's artwork, under the MIT
    /// license (see THIRD-PARTY-NOTICES.txt of the theme).
    /// </summary>
    public const string Banner =
        "/*! The check box, radio button and switch icons are the artwork of Bootstrap v5.3.8 (https://getbootstrap.com/), "
        + "Copyright 2011-2025 The Bootstrap Authors, Licensed under MIT (https://github.com/twbs/bootstrap/blob/main/LICENSE). "
        + "See THIRD-PARTY-NOTICES.txt in the package of the Admin theme. */\n";

    /// <summary>
    /// Appends the rules of one scheme; appends nothing for a scheme with an invalid id or an invalid color.
    /// </summary>
    public static void AppendScheme(StringBuilder css, AdminScheme scheme)
    {
        ArgumentNullException.ThrowIfNull(css);

        if (scheme is null || !AdminSchemeStore.IsValidId(scheme.Id))
        {
            return;
        }

        var palette = AdminSchemePalette.FromScheme(scheme);

        if (palette is null)
        {
            return;
        }

        AppendRule(css, GetSelector(scheme.Id), BuildTokens(palette, AdminSchemeMode.Light));
        AppendRule(css, TheAdminSchemes.DarkModeSelector + GetSelector(scheme.Id), BuildTokens(palette, AdminSchemeMode.Dark));
        TheAdminSchemeTokens.AppendRules(css, scheme, palette);
    }

    /// <summary>
    /// The custom properties of one rule of a scheme: the generated tokens, then the layout's.
    /// </summary>
    public static IReadOnlyList<KeyValuePair<string, string>> BuildTokens(AdminSchemePalette palette, AdminSchemeMode mode)
    {
        ArgumentNullException.ThrowIfNull(palette);

        var tokens = new AdminSchemeTokenWriter();
        WriteTokens(palette, mode, tokens);
        TheAdminSchemeTokens.WriteTokens(palette, mode, tokens);

        return tokens.Variables;
    }

    /// <summary>
    /// Writes the generated tokens of a mode: the brand colors, links and focus, surfaces, text, borders, and (in the
    /// light rule, as both modes share them) the typography and shape settings. The dark rule holds only what changes in
    /// dark mode.
    /// </summary>
    public static void WriteTokens(AdminSchemePalette palette, AdminSchemeMode mode, AdminSchemeTokenWriter tokens)
    {
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(tokens);

        var colors = palette.Get(mode);

        if (mode == AdminSchemeMode.Light)
        {
            tokens.AddWithRgb("primary", colors.Primary);
        }

        tokens.Add("primary-hover", colors.PrimaryHover)
            .Add("primary-active", colors.PrimaryActive)
            .Add("primary-subtle", colors.PrimarySubtle)
            .Add("primary-border-subtle", colors.PrimaryBorderSubtle)
            .Add("primary-emphasis", colors.PrimaryEmphasis);

        if (mode == AdminSchemeMode.Light)
        {
            tokens.Add("on-primary", palette.OnPrimary)
                .AddWithRgb("deep", palette.Deep)
                .Add("ink", palette.Ink)
                .AddWithRgb("accent", palette.Accent);
        }

        tokens.AddWithRgb("link", colors.Link)
            .AddWithRgb("link-hover", colors.LinkHover)
            .Add("focus", colors.Focus)
            .Add("page", colors.Page)
            .AddWithRgb("surface", colors.Surface)
            .Add("surface-muted", colors.SurfaceMuted);

        if (colors.SecondaryBackground is { } secondary)
        {
            tokens.Add("secondary-bg", secondary);
        }

        if (colors.BodyColor is { } bodyColor)
        {
            tokens.AddWithRgb("body-color", bodyColor);
        }

        tokens.Add("heading", colors.Heading)
            .Add("label", colors.Label)
            .Add("muted", colors.Muted)
            .Add("border", colors.Border)
            .Add("control-border", colors.ControlBorder)
            .Add("control-checked-border", colors.ControlCheckedBorder)
            .Add("row-hover", colors.RowHover);

        if (mode != AdminSchemeMode.Light)
        {
            return;
        }

        var heading = AdminSchemeTypography.GetFontStack(palette.HeadingFontFamily, TheAdminSchemes.Fonts);

        tokens.Add("font-family", TheAdminSchemes.GetFontStack(palette.FontFamily))
            .Add("heading-font-family", heading ?? tokens.Var("font-family"))
            .Add("font-scale", AdminSchemeTypography.Format(palette.FontScale))
            .Add("radius-scale", AdminSchemeTypography.Format(palette.RadiusScale))
            .Add("density", AdminSchemeTypography.Format(palette.Density));
    }

    /// <summary>
    /// The selector of a scheme's light rule: <c>[data-ta-scheme="{id}"]</c>.
    /// </summary>
    public static string GetSelector(string schemeId)
        => $"[{TheAdminSchemes.AttributeName}=\"{schemeId}\"]";

    /// <summary>
    /// The version of a stylesheet: the first 16 hexadecimal characters of the SHA-256 of its text, which changes when
    /// either the schemes or the code that writes them change.
    /// </summary>
    public static string ComputeVersion(string css)
        => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(css ?? string.Empty)))[..16];

    private static void AppendRule(StringBuilder css, string selector, IReadOnlyList<KeyValuePair<string, string>> tokens)
    {
        css.Append(selector).Append('{');

        foreach (var token in tokens)
        {
            css.Append(token.Key).Append(':').Append(token.Value).Append(';');
        }

        css.Append("}\n");
    }
}
