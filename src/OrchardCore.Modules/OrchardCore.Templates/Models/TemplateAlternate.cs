namespace OrchardCore.Templates.Models;

/// <summary>
/// An alternate of a shape: a more specific template name that renders the shape in some cases only,
/// e.g. <c>Content_Summary__BlogPost</c> renders the <c>Content</c> shape of the blog posts in the summary display type.
/// </summary>
public sealed class TemplateAlternate
{
    /// <summary>
    /// Gets or sets the name of the alternate, which is the name of the template to create.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the shape type the alternate is for, without display type, e.g. <c>Content</c>, <c>TitlePart</c> or <c>TextField</c>.
    /// </summary>
    public string ShapeType { get; set; }

    /// <summary>
    /// Gets or sets the display type the alternate is rendered in, e.g. <c>Detail</c> or <c>Summary</c>.
    /// </summary>
    public string DisplayType { get; set; }

    /// <summary>
    /// Gets or sets the content type the alternate is rendered for, or <see langword="null"/> when it applies to several.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets what the alternate renders in the content type, e.g. <c>HtmlBodyPart</c> for a part or
    /// <c>HtmlBodyPart.Description</c> for a field, or <see langword="null"/> for the content item itself.
    /// </summary>
    public string Element { get; set; }
}
