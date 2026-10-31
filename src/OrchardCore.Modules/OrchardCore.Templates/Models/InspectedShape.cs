namespace OrchardCore.Templates.Models;

/// <summary>
/// A shape rendered while inspecting a content item, with its alternates and the template that renders it.
/// </summary>
public sealed class InspectedShape
{
    /// <summary>
    /// Gets or sets the type of the shape, e.g. <c>Content</c> or <c>TextField</c>.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the display type the shape is rendered in, e.g. <c>Detail</c>.
    /// </summary>
    public string DisplayType { get; set; }

    /// <summary>
    /// Gets or sets the differentiator of the shape in its zone, e.g. the name of a part or a field.
    /// </summary>
    public string Differentiator { get; set; }

    /// <summary>
    /// Gets or sets how deep the shape is in the rendered tree, 0 for the content item.
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Gets or sets the alternates of the shape, the most specific first.
    /// </summary>
    public IReadOnlyList<string> Alternates { get; set; } = [];

    /// <summary>
    /// Gets or sets the name the template rendering the shape is bound to, its type or one of its alternates.
    /// </summary>
    public string RenderedWith { get; set; }

    /// <summary>
    /// Gets or sets whether a local template renders the shape.
    /// </summary>
    public bool IsLocal { get; set; }

    /// <summary>
    /// Gets or sets the extension of the template file rendering the shape, when a file renders it.
    /// </summary>
    public string ExtensionId { get; set; }

    /// <summary>
    /// Gets or sets the path of the template file rendering the shape inside its extension, or what renders it otherwise,
    /// e.g. the method of a shape defined in code.
    /// </summary>
    public string Source { get; set; }
}
