using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes;

/// <summary>
/// Writes Bootstrap 5.3's root variables from the generated tokens, as references to them: the primary, links, the
/// body, secondary text, borders, headings, the focus ring, the body font and the border radius scale. Enable it with
/// <see cref="ThemeSchemeOptions.UseBootstrap"/>. The colors are written in the dark rule too, so they win over
/// Bootstrap's own dark mode rule; Bootstrap's components that compile their colors in (e.g. <c>.btn-primary</c>)
/// still need the theme's CSS to read the tokens.
/// </summary>
public sealed class BootstrapThemeSchemeTokens : IThemeSchemeTokenContributor
{
    /// <inheritdoc/>
    public void WriteTokens(ThemeScheme scheme, ThemeSchemePalette palette, ThemeSchemeMode mode, ThemeSchemeTokenWriter tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        tokens.AddVariable("--bs-primary", tokens.Var("primary"))
            .AddVariable("--bs-primary-rgb", tokens.VarRgb("primary"))
            .AddVariable("--bs-primary-text-emphasis", tokens.Var("primary-emphasis"))
            .AddVariable("--bs-primary-bg-subtle", tokens.Var("primary-subtle"))
            .AddVariable("--bs-primary-border-subtle", tokens.Var("primary-border-subtle"))
            .AddVariable("--bs-link-color", tokens.Var("link"))
            .AddVariable("--bs-link-color-rgb", tokens.VarRgb("link"))
            .AddVariable("--bs-link-hover-color", tokens.Var("link-hover"))
            .AddVariable("--bs-link-hover-color-rgb", tokens.VarRgb("link-hover"))
            .AddVariable("--bs-body-bg", tokens.Var("page"))
            .AddVariable("--bs-secondary-color", tokens.Var("muted"))
            .AddVariable("--bs-border-color", tokens.Var("border"))
            .AddVariable("--bs-heading-color", tokens.Var("heading"))
            .AddVariable("--bs-focus-ring-color", $"rgba({tokens.VarRgb("link")}, 0.25)");

        if (mode == ThemeSchemeMode.Dark)
        {
            // Only the dark mode has generated body and secondary colors: the light mode keeps Bootstrap's.
            tokens.AddVariable("--bs-body-color", tokens.Var("body-color"))
                .AddVariable("--bs-body-color-rgb", tokens.VarRgb("body-color"))
                .AddVariable("--bs-secondary-bg", tokens.Var("secondary-bg"));

            return;
        }

        var radius = tokens.Var("radius-scale");

        tokens.AddVariable("--bs-body-font-family", tokens.Var("font-family"))
            .AddVariable("--bs-border-radius", $"calc(0.375rem * {radius})")
            .AddVariable("--bs-border-radius-sm", $"calc(0.25rem * {radius})")
            .AddVariable("--bs-border-radius-lg", $"calc(0.5rem * {radius})")
            .AddVariable("--bs-border-radius-xl", $"calc(1rem * {radius})")
            .AddVariable("--bs-border-radius-xxl", $"calc(2rem * {radius})");
    }
}
