namespace OrchardCore.Themes.Schemes.Models;

/// <summary>
/// A scheme of a theme: a name and four colors, from which the theme derives its color tokens, light and dark, and
/// the typography and shape of its pages. The optional settings fine tune what is derived; left empty, they are worked
/// out from the four colors or take their default (see <see cref="ThemeSchemePalette"/>). Each theme uses the
/// settings it understands.
/// </summary>
public sealed class ThemeScheme
{
    /// <summary>
    /// The id: the value of the scheme attribute of the page (see <see cref="ThemeSchemeOptions.AttributeName"/>).
    /// Lowercase letters, digits and dashes.
    /// </summary>
    public string Id { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// The place of the scheme in the pickers, lowest first.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Actions, links and the current page, as '#rrggbb'.
    /// </summary>
    public string Primary { get; set; }

    /// <summary>
    /// Headings and the top of the menu, as '#rrggbb'.
    /// </summary>
    public string Deep { get; set; }

    /// <summary>
    /// The bottom of the menu, the darkest surface, as '#rrggbb'.
    /// </summary>
    public string Ink { get; set; }

    /// <summary>
    /// Links and highlights on dark surfaces, as '#rrggbb'.
    /// </summary>
    public string Accent { get; set; }

    /// <summary>
    /// Whether the menu is light gray with dark text rather than dark on the scheme's colors.
    /// </summary>
    public bool LightMenu { get; set; }

    /// <summary>
    /// The hue of the page, borders and dark mode surfaces, 0 to 360; the deep color's when empty.
    /// </summary>
    public double? Hue { get; set; }

    /// <summary>
    /// How saturated the page, borders and dark mode surfaces are, 0 (gray) to 1; from the deep color when empty.
    /// </summary>
    public double? Saturation { get; set; }

    /// <summary>
    /// The color of text on the primary, as '#rrggbb'; white, or dark on a light primary, when empty.
    /// </summary>
    public string OnPrimary { get; set; }

    /// <summary>
    /// The page in light mode, as '#rrggbb'; a pale tone of the hue when empty.
    /// </summary>
    public string Page { get; set; }

    /// <summary>
    /// The toolbar of lists, as '#rrggbb'; the primary when empty.
    /// </summary>
    public string Toolbar { get; set; }

    /// <summary>
    /// The color of both ends of the page gradient in light mode, as '#rrggbb'; the primary and the accent when empty.
    /// </summary>
    public string Tint { get; set; }

    /// <summary>
    /// The color of both ends of the page gradient in dark mode, as '#rrggbb'; the accent and the primary when empty.
    /// </summary>
    public string TintDark { get; set; }

    /// <summary>
    /// The body font, one of the keys of <see cref="ThemeSchemeTypography.Fonts"/> ('lato', 'inter', 'source-sans-3',
    /// 'system'); Lato when empty.
    /// </summary>
    public string FontFamily { get; set; }

    /// <summary>
    /// The font of headings, one of the keys of <see cref="ThemeSchemeTypography.Fonts"/>; the body font when empty.
    /// </summary>
    public string HeadingFontFamily { get; set; }

    /// <summary>
    /// The scale of the base font size, 0.875 (14 pixels) to 1.125 (18 pixels); 1 (16 pixels) when empty.
    /// </summary>
    public double? FontScale { get; set; }

    /// <summary>
    /// The scale of the corner radii, 0 (square) to 1.5 (round); 1 when empty.
    /// </summary>
    public double? RadiusScale { get; set; }

    /// <summary>
    /// The scale of the spacing of controls and lists, 0.75 (compact) to 1.25 (comfortable); 1 when empty.
    /// </summary>
    public double? Density { get; set; }
}
