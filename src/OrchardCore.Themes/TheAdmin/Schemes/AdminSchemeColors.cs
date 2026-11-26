namespace OrchardCore.Themes.TheAdmin.Schemes;

/// <summary>
/// The colors of a scheme in one color mode, derived by <see cref="AdminSchemePalette"/>, with text and control
/// boundaries already raised to WCAG 2.2 AA contrast against every surface in <see cref="Surfaces"/>.
/// </summary>
public sealed class AdminSchemeColors
{
    /// <summary>
    /// Gets the primary, darkened when needed so that <see cref="AdminSchemePalette.OnPrimary"/> text reads on it.
    /// </summary>
    public AdminColor Primary { get; init; }

    /// <summary>
    /// Gets the primary under the pointer.
    /// </summary>
    public AdminColor PrimaryHover { get; init; }

    /// <summary>
    /// Gets the primary while pressed.
    /// </summary>
    public AdminColor PrimaryActive { get; init; }

    /// <summary>
    /// Gets a faint tone of the primary, for the background of highlighted items.
    /// </summary>
    public AdminColor PrimarySubtle { get; init; }

    /// <summary>
    /// Gets the border of highlighted items.
    /// </summary>
    public AdminColor PrimaryBorderSubtle { get; init; }

    /// <summary>
    /// Gets the text of highlighted items.
    /// </summary>
    public AdminColor PrimaryEmphasis { get; init; }

    /// <summary>
    /// Gets the color of links.
    /// </summary>
    public AdminColor Link { get; init; }

    /// <summary>
    /// Gets the color of links under the pointer.
    /// </summary>
    public AdminColor LinkHover { get; init; }

    /// <summary>
    /// Gets the color of focus rings.
    /// </summary>
    public AdminColor Focus { get; init; }

    /// <summary>
    /// Gets the background of the page.
    /// </summary>
    public AdminColor Page { get; init; }

    /// <summary>
    /// Gets the background of cards, panels and fields.
    /// </summary>
    public AdminColor Surface { get; init; }

    /// <summary>
    /// Gets a quieter surface, e.g. for the footer of a card.
    /// </summary>
    public AdminColor SurfaceMuted { get; init; }

    /// <summary>
    /// Gets the background of secondary elements in dark mode; <see langword="null"/> in light mode.
    /// </summary>
    public AdminColor? SecondaryBackground { get; init; }

    /// <summary>
    /// Gets the color of body text in dark mode; <see langword="null"/> in light mode, where the theme keeps its own.
    /// </summary>
    public AdminColor? BodyColor { get; init; }

    /// <summary>
    /// Gets the color of headings.
    /// </summary>
    public AdminColor Heading { get; init; }

    /// <summary>
    /// Gets the color of form labels.
    /// </summary>
    public AdminColor Label { get; init; }

    /// <summary>
    /// Gets the color of secondary text and hints.
    /// </summary>
    public AdminColor Muted { get; init; }

    /// <summary>
    /// Gets the color of borders and dividers.
    /// </summary>
    public AdminColor Border { get; init; }

    /// <summary>
    /// Gets the border of fields, check boxes and other controls.
    /// </summary>
    public AdminColor ControlBorder { get; init; }

    /// <summary>
    /// Gets the border of a checked check box, radio button or switch.
    /// </summary>
    public AdminColor ControlCheckedBorder { get; init; }

    /// <summary>
    /// Gets the background of a row of a list or table under the pointer.
    /// </summary>
    public AdminColor RowHover { get; init; }

    /// <summary>
    /// Gets the start of a soft gradient over the page, from the scheme's tint.
    /// </summary>
    public AdminColor PageTintStart { get; init; }

    /// <summary>
    /// Gets the end of a soft gradient over the page, from the scheme's tint.
    /// </summary>
    public AdminColor PageTintEnd { get; init; }

    /// <summary>
    /// Gets the background of the headers of lists and tables.
    /// </summary>
    public AdminColor HeaderSurface { get; init; }

    /// <summary>
    /// Gets every surface text is checked against: the page, the surfaces, the page tint, the header surface and the
    /// hovered row.
    /// </summary>
    public IReadOnlyList<AdminColor> Surfaces { get; init; } = [];
}
