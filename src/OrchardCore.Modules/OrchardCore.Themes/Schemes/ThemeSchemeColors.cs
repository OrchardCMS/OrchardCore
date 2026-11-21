namespace OrchardCore.Themes.Schemes;

/// <summary>
/// The colors of a scheme in one color mode, derived by <see cref="ThemeSchemePalette"/>, with text and control
/// boundaries already raised to WCAG 2.2 AA contrast against every surface in <see cref="Surfaces"/>.
/// </summary>
public sealed class ThemeSchemeColors
{
    /// <summary>
    /// Gets the primary, darkened when needed so that <see cref="ThemeSchemePalette.OnPrimary"/> text reads on it.
    /// </summary>
    public ThemeColor Primary { get; init; }

    /// <summary>
    /// Gets the primary under the pointer.
    /// </summary>
    public ThemeColor PrimaryHover { get; init; }

    /// <summary>
    /// Gets the primary while pressed.
    /// </summary>
    public ThemeColor PrimaryActive { get; init; }

    /// <summary>
    /// Gets a faint tone of the primary, for the background of highlighted items.
    /// </summary>
    public ThemeColor PrimarySubtle { get; init; }

    /// <summary>
    /// Gets the border of highlighted items.
    /// </summary>
    public ThemeColor PrimaryBorderSubtle { get; init; }

    /// <summary>
    /// Gets the text of highlighted items.
    /// </summary>
    public ThemeColor PrimaryEmphasis { get; init; }

    /// <summary>
    /// Gets the color of links.
    /// </summary>
    public ThemeColor Link { get; init; }

    /// <summary>
    /// Gets the color of links under the pointer.
    /// </summary>
    public ThemeColor LinkHover { get; init; }

    /// <summary>
    /// Gets the color of focus rings.
    /// </summary>
    public ThemeColor Focus { get; init; }

    /// <summary>
    /// Gets the background of the page.
    /// </summary>
    public ThemeColor Page { get; init; }

    /// <summary>
    /// Gets the background of cards, panels and fields.
    /// </summary>
    public ThemeColor Surface { get; init; }

    /// <summary>
    /// Gets a quieter surface, e.g. for the footer of a card.
    /// </summary>
    public ThemeColor SurfaceMuted { get; init; }

    /// <summary>
    /// Gets the background of secondary elements in dark mode; <see langword="null"/> in light mode.
    /// </summary>
    public ThemeColor? SecondaryBackground { get; init; }

    /// <summary>
    /// Gets the color of body text in dark mode; <see langword="null"/> in light mode, where the theme keeps its own.
    /// </summary>
    public ThemeColor? BodyColor { get; init; }

    /// <summary>
    /// Gets the color of headings.
    /// </summary>
    public ThemeColor Heading { get; init; }

    /// <summary>
    /// Gets the color of form labels.
    /// </summary>
    public ThemeColor Label { get; init; }

    /// <summary>
    /// Gets the color of secondary text and hints.
    /// </summary>
    public ThemeColor Muted { get; init; }

    /// <summary>
    /// Gets the color of borders and dividers.
    /// </summary>
    public ThemeColor Border { get; init; }

    /// <summary>
    /// Gets the border of fields, check boxes and other controls.
    /// </summary>
    public ThemeColor ControlBorder { get; init; }

    /// <summary>
    /// Gets the border of a checked check box, radio button or switch.
    /// </summary>
    public ThemeColor ControlCheckedBorder { get; init; }

    /// <summary>
    /// Gets the background of a row of a list or table under the pointer.
    /// </summary>
    public ThemeColor RowHover { get; init; }

    /// <summary>
    /// Gets the start of a soft gradient over the page, from the scheme's tint.
    /// </summary>
    public ThemeColor PageTintStart { get; init; }

    /// <summary>
    /// Gets the end of a soft gradient over the page, from the scheme's tint.
    /// </summary>
    public ThemeColor PageTintEnd { get; init; }

    /// <summary>
    /// Gets the background of the headers of lists and tables.
    /// </summary>
    public ThemeColor HeaderSurface { get; init; }

    /// <summary>
    /// Gets every surface text is checked against: the page, the surfaces, the page tint, the header surface and the
    /// hovered row.
    /// </summary>
    public IReadOnlyList<ThemeColor> Surfaces { get; init; } = [];
}
