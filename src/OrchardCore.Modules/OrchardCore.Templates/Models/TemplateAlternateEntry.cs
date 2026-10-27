namespace OrchardCore.Templates.Models;

/// <summary>
/// A row of the alternates list of a template: an alternate and the state of its local template.
/// </summary>
public sealed class TemplateAlternateEntry
{
    /// <summary>
    /// Gets or sets the alternate.
    /// </summary>
    public TemplateAlternate Alternate { get; set; }

    /// <summary>
    /// Gets or sets the template the alternate is of, as listed by its source.
    /// </summary>
    public SourceTemplateEntry Base { get; set; }

    /// <summary>
    /// Gets or sets whether a local template with the name of the alternate exists.
    /// </summary>
    public bool IsOverridden { get; set; }

    /// <summary>
    /// Gets or sets the extension whose template already renders the alternate, e.g. a theme shipping
    /// <c>Content-BlogPost.liquid</c>, or <see langword="null"/> when the shape renders with the template it is the alternate of.
    /// </summary>
    public string RenderedBy { get; set; }

    /// <summary>
    /// Gets or sets the template source listing the template that renders the alternate, to view it.
    /// </summary>
    public string RenderedBySource { get; set; }
}
