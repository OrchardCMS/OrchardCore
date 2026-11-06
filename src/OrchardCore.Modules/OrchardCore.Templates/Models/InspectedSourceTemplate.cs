namespace OrchardCore.Templates.Models;

/// <summary>
/// A template of a template source, e.g. of the active theme, bound to a shape type or an alternate of an inspected shape.
/// </summary>
public sealed class InspectedSourceTemplate
{
    /// <summary>
    /// Gets or sets the name of the source of the template, e.g. <c>Theme</c>.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the template.
    /// </summary>
    public SourceTemplate Template { get; set; }
}
