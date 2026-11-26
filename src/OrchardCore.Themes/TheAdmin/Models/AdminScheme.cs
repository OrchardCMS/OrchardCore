namespace OrchardCore.Themes.TheAdmin.Models;

/// <summary>
/// A scheme of TheAdmin: a name and four colors, from which the theme derives its color tokens, light and dark, and
/// the typography and shape of its pages. The optional settings fine tune what is derived; left empty, they are worked
/// out from the four colors or take their default (see <see cref="Schemes.AdminSchemePalette"/>).
/// </summary>
public sealed class AdminScheme
{
    /// <summary>
    /// The id: the value of the scheme attribute of the page (see <see cref="Schemes.TheAdminSchemes.AttributeName"/>).
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
    /// Whether the admin forms are laid out horizontally, with the label of a field beside its editor instead of above
    /// it, on screens at least as wide as a tablet. Off by default: the forms are stacked.
    /// </summary>
    public bool HorizontalFormLayout { get; set; }

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
    /// The body font: the key or the name of one of the theme's fonts (<see cref="Schemes.TheAdminSchemes.Fonts"/>, e.g.
    /// 'lato' or 'Lato'), 'system' for the system font, or a list of font families separated by commas (e.g.
    /// 'Georgia, serif'). The theme's default font when empty (<see cref="Schemes.TheAdminSchemes.DefaultFontFamily"/>).
    /// </summary>
    public string FontFamily { get; set; }

    /// <summary>
    /// The font of headings, written like <see cref="FontFamily"/>; the body font when empty.
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
