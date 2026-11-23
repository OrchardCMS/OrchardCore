namespace OrchardCore.Themes.Schemes;

/// <summary>
/// The settings of a scheme, beyond its colors, that a theme's stylesheet reads. The editor of schemes only shows the
/// ones the theme supports, and the stylesheet only writes their tokens. See <see cref="ThemeSchemeOptions.Features"/>.
/// </summary>
[Flags]
public enum ThemeSchemeFeatures
{
    /// <summary>
    /// Colors only.
    /// </summary>
    None = 0,

    /// <summary>
    /// The font of the text and the font of headings: the <c>font-family</c> and <c>heading-font-family</c> tokens.
    /// </summary>
    Fonts = 1,

    /// <summary>
    /// The base size of the text: the <c>font-scale</c> token.
    /// </summary>
    FontSize = 2,

    /// <summary>
    /// How round the corners are: the <c>radius-scale</c> token.
    /// </summary>
    Corners = 4,

    /// <summary>
    /// How much space controls and rows take: the <c>density</c> token.
    /// </summary>
    Density = 8,

    /// <summary>
    /// Every setting.
    /// </summary>
    All = Fonts | FontSize | Corners | Density,
}
