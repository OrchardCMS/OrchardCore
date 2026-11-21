using System.Text;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes;

/// <summary>
/// Adds tokens, and optionally rules, to the stylesheet of a theme's schemes, after the generated ones. Register it with
/// <see cref="ThemeSchemeOptions.AddTokens{T}"/>. It is created through dependency injection each time the
/// stylesheet is built (once per change of the schemes), so it can take services.
/// </summary>
public interface IThemeSchemeTokenContributor
{
    /// <summary>
    /// Writes the tokens of a scheme for one mode. Called with <see cref="ThemeSchemeMode.Light"/> for the rule on
    /// <c>[{AttributeName}="{id}"]</c>, which also holds what both modes share, then with
    /// <see cref="ThemeSchemeMode.Dark"/> for the dark rule, unless the theme has no dark mode
    /// (<see cref="ThemeSchemeOptions.DarkModeSelector"/> is <see langword="null"/>).
    /// </summary>
    /// <param name="scheme">The scheme, as stored.</param>
    /// <param name="palette">The scheme resolved into colors, typography and shape.</param>
    /// <param name="mode">The mode of the rule being written.</param>
    /// <param name="tokens">The writer of the rule: tokens go under the theme's prefix.</param>
    void WriteTokens(ThemeScheme scheme, ThemeSchemePalette palette, ThemeSchemeMode mode, ThemeSchemeTokenWriter tokens);

    /// <summary>
    /// Appends rules of its own after the rules of the scheme, e.g. for a swatch drawn outside the page's scheme. The
    /// scheme's id is a valid CSS identifier. Writes nothing by default.
    /// </summary>
    /// <param name="css">The stylesheet being built.</param>
    /// <param name="scheme">The scheme, as stored.</param>
    /// <param name="palette">The scheme resolved into colors, typography and shape.</param>
    void AppendRules(StringBuilder css, ThemeScheme scheme, ThemeSchemePalette palette)
    {
    }
}
