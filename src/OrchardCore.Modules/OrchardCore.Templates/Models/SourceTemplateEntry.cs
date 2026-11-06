namespace OrchardCore.Templates.Models;

/// <summary>
/// A row of a template source list: a source template and the state of its local override.
/// </summary>
public sealed class SourceTemplateEntry
{
    /// <summary>
    /// Gets or sets the name of the template source listing the template, e.g. <c>Theme</c>.
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    /// Gets or sets the name of the scope of the source listing the template, e.g. a theme, which the row links carry in their route.
    /// </summary>
    public string Scope { get; set; }

    /// <summary>
    /// Gets or sets the template shipped by the source.
    /// </summary>
    public SourceTemplate Template { get; set; }

    /// <summary>
    /// Gets or sets whether the entry belongs to the admin templates, which the row links carry in their route.
    /// </summary>
    public bool AdminTemplates { get; set; }

    /// <summary>
    /// Gets or sets whether a local template overrides the source template.
    /// </summary>
    public bool IsOverridden { get; set; }

    /// <summary>
    /// Gets or sets whether the local template was cloned from the source template, rather than written by hand.
    /// </summary>
    public bool IsCloned { get; set; }

    /// <summary>
    /// Gets or sets the extension the local template was cloned from, when it is another one than the extension
    /// of the template, e.g. a theme that was active before.
    /// </summary>
    public string ClonedFrom { get; set; }

    /// <summary>
    /// Gets or sets whether the shape of the template has alternates, e.g. <c>Content__BlogPost</c> for <c>Content</c>.
    /// </summary>
    public bool HasAlternates { get; set; }

    /// <summary>
    /// Gets or sets whether the cloned template was edited since it was cloned.
    /// </summary>
    public bool IsModified { get; set; }

    /// <summary>
    /// Gets or sets whether the source template changed since it was cloned, e.g. after the theme was updated.
    /// </summary>
    public bool IsSourceChanged { get; set; }
}
