using System.Text;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes;

/// <summary>
/// What <see cref="ThemeSchemeOptions.CssWriter"/> writes the CSS of one scheme with.
/// </summary>
public sealed class ThemeSchemeCssContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeSchemeCssContext"/> class.
    /// </summary>
    public ThemeSchemeCssContext(
        StringBuilder css,
        ThemeScheme scheme,
        ThemeSchemePalette palette,
        ThemeSchemeOptions options,
        IReadOnlyList<IThemeSchemeTokenContributor> contributors)
    {
        ArgumentNullException.ThrowIfNull(css);
        ArgumentNullException.ThrowIfNull(scheme);
        ArgumentNullException.ThrowIfNull(palette);
        ArgumentNullException.ThrowIfNull(options);

        Css = css;
        Scheme = scheme;
        Palette = palette;
        Options = options;
        Contributors = contributors ?? [];
    }

    /// <summary>
    /// Gets the stylesheet being built: append the scheme's rules to it.
    /// </summary>
    public StringBuilder Css { get; }

    /// <summary>
    /// Gets the scheme. Its id is a valid CSS identifier (lowercase letters, digits and dashes).
    /// </summary>
    public ThemeScheme Scheme { get; }

    /// <summary>
    /// Gets the scheme resolved into colors, typography and shape.
    /// </summary>
    public ThemeSchemePalette Palette { get; }

    /// <summary>
    /// Gets the options of the theme.
    /// </summary>
    public ThemeSchemeOptions Options { get; }

    /// <summary>
    /// Gets the contributors of the theme, created for this build.
    /// </summary>
    public IReadOnlyList<IThemeSchemeTokenContributor> Contributors { get; }

    /// <summary>
    /// The selector of the scheme's light rule: <c>[{AttributeName}="{id}"]</c>.
    /// </summary>
    public string Selector
        => ThemeSchemeStylesheet.GetSelector(Options, Scheme.Id);

    /// <summary>
    /// Appends what is written when there is no <see cref="ThemeSchemeOptions.CssWriter"/>: the generated tokens and
    /// the contributors' tokens and rules.
    /// </summary>
    public void WriteDefault()
        => ThemeSchemeStylesheet.AppendDefault(Css, Scheme, Palette, Options, Contributors);
}
