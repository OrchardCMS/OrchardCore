using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Themes.Schemes.Models;
using OrchardCore.Themes.Schemes.Services;

namespace OrchardCore.Themes.Schemes;

/// <summary>
/// Writes the stylesheet of a theme's schemes. For each scheme with a valid id and valid colors it writes the
/// generated tokens (<see cref="WriteTokens"/>) and the contributors' ones under
/// <c>[{AttributeName}="{id}"]</c> for the light mode, and under <c>{DarkModeSelector}[{AttributeName}="{id}"]</c> for
/// the dark mode, then the contributors' own rules; or whatever the theme's <see cref="ThemeSchemeOptions.CssWriter"/>
/// writes instead. It assumes no CSS framework.
/// </summary>
public static class ThemeSchemeStylesheet
{
    private static readonly IServiceProvider s_noServices = new ServiceCollection().BuildServiceProvider();

    /// <summary>
    /// Builds the stylesheet of the given schemes; the schemes with an invalid id or an invalid color are skipped.
    /// </summary>
    /// <param name="options">The options of the theme.</param>
    /// <param name="schemes">The schemes.</param>
    /// <param name="services">The services the contributors are created with; none when <see langword="null"/>.</param>
    public static string Build(ThemeSchemeOptions options, IEnumerable<ThemeScheme> schemes, IServiceProvider services = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(schemes);

        var contributors = CreateContributors(options, services);
        var css = new StringBuilder();

        foreach (var scheme in schemes)
        {
            AppendScheme(css, scheme, options, contributors);
        }

        return css.ToString();
    }

    /// <summary>
    /// Creates the contributors of the theme, through dependency injection.
    /// </summary>
    public static IReadOnlyList<IThemeSchemeTokenContributor> CreateContributors(ThemeSchemeOptions options, IServiceProvider services = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        return [.. options.TokenContributors.Select(type => (IThemeSchemeTokenContributor)ActivatorUtilities.CreateInstance(services ?? s_noServices, type))];
    }

    /// <summary>
    /// Appends the rules of one scheme; appends nothing for a scheme with an invalid id or an invalid color.
    /// </summary>
    public static void AppendScheme(StringBuilder css, ThemeScheme scheme, ThemeSchemeOptions options, IReadOnlyList<IThemeSchemeTokenContributor> contributors)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(options);

        if (scheme is null || !ThemeSchemeStore.IsValidId(scheme.Id))
        {
            return;
        }

        var palette = ThemeSchemePalette.FromScheme(scheme);

        if (palette is null)
        {
            return;
        }

        if (options.CssWriter is not null)
        {
            options.CssWriter(new ThemeSchemeCssContext(css, scheme, palette, options, contributors));

            return;
        }

        AppendDefault(css, scheme, palette, options, contributors);
    }

    /// <summary>
    /// Appends the generated rules of one scheme: the generated and the contributors' tokens, light then dark, then the
    /// contributors' rules.
    /// </summary>
    public static void AppendDefault(StringBuilder css, ThemeScheme scheme, ThemeSchemePalette palette, ThemeSchemeOptions options, IReadOnlyList<IThemeSchemeTokenContributor> contributors)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(scheme);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(options);

        contributors ??= [];

        AppendRule(css, GetSelector(options, scheme.Id), BuildTokens(scheme, palette, ThemeSchemeMode.Light, options, contributors));

        if (!string.IsNullOrEmpty(options.DarkModeSelector))
        {
            AppendRule(css, options.DarkModeSelector + GetSelector(options, scheme.Id), BuildTokens(scheme, palette, ThemeSchemeMode.Dark, options, contributors));
        }

        foreach (var contributor in contributors)
        {
            contributor.AppendRules(css, scheme, palette);
        }
    }

    /// <summary>
    /// The custom properties of one rule of a scheme: the generated tokens, then the contributors' ones.
    /// </summary>
    public static IReadOnlyList<KeyValuePair<string, string>> BuildTokens(
        ThemeScheme scheme,
        ThemeSchemePalette palette,
        ThemeSchemeMode mode,
        ThemeSchemeOptions options,
        IReadOnlyList<IThemeSchemeTokenContributor> contributors = null)
    {
        ArgumentNullException.ThrowIfNull(scheme);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(options);

        var tokens = new ThemeSchemeTokenWriter(options.TokenPrefix, options.TokenNames);
        WriteTokens(palette, mode, tokens);

        foreach (var contributor in contributors ?? [])
        {
            contributor.WriteTokens(scheme, palette, mode, tokens);
        }

        return tokens.Variables;
    }

    /// <summary>
    /// Writes the generated tokens of a mode: the brand colors, links and focus, surfaces, text, borders, and (in the
    /// light rule, as both modes share them) the typography and shape. The dark rule holds only what changes in dark
    /// mode.
    /// </summary>
    public static void WriteTokens(ThemeSchemePalette palette, ThemeSchemeMode mode, ThemeSchemeTokenWriter tokens)
    {
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(tokens);

        var colors = palette.Get(mode);

        if (mode == ThemeSchemeMode.Light)
        {
            tokens.AddWithRgb("primary", colors.Primary);
        }

        tokens.Add("primary-hover", colors.PrimaryHover)
            .Add("primary-active", colors.PrimaryActive)
            .Add("primary-subtle", colors.PrimarySubtle)
            .Add("primary-border-subtle", colors.PrimaryBorderSubtle)
            .Add("primary-emphasis", colors.PrimaryEmphasis);

        if (mode == ThemeSchemeMode.Light)
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

        if (mode == ThemeSchemeMode.Light)
        {
            var body = ThemeSchemeTypography.FindFont(palette.FontFamily)
                ?? ThemeSchemeTypography.FindFont(ThemeSchemeTypography.DefaultFontFamily);
            var heading = ThemeSchemeTypography.FindFont(palette.HeadingFontFamily);

            tokens.Add("font-family", body.Stack)
                .Add("heading-font-family", heading?.Stack ?? tokens.Var("font-family"))
                .Add("font-scale", ThemeSchemeTypography.Format(palette.FontScale))
                .Add("radius-scale", ThemeSchemeTypography.Format(palette.RadiusScale))
                .Add("density", ThemeSchemeTypography.Format(palette.Density));
        }
    }

    /// <summary>
    /// The selector of a scheme's light rule: <c>[{AttributeName}="{id}"]</c>.
    /// </summary>
    public static string GetSelector(ThemeSchemeOptions options, string schemeId)
    {
        ArgumentNullException.ThrowIfNull(options);

        return $"[{options.AttributeName}=\"{schemeId}\"]";
    }

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
