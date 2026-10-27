namespace OrchardCore.Templates.Models;

/// <summary>
/// A shape template file shipped by an extension (a theme or a module), which a local template can override.
/// </summary>
public sealed class SourceTemplate
{
    /// <summary>
    /// Gets or sets the name of the shape the template renders, which is also the name of the local template overriding it.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the extension shipping the template, e.g. <c>TheBlogTheme</c>.
    /// </summary>
    public string ExtensionId { get; set; }

    /// <summary>
    /// Gets or sets the path of the file inside its extension, e.g. <c>Views/Content-Article.liquid</c>.
    /// </summary>
    public string RelativePath { get; set; }

    /// <summary>
    /// Gets or sets the path of the file in the shape templates file provider, used to read its content.
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// Gets whether the template is written in Liquid, so that it can be cloned as a local template.
    /// Other templates, e.g. Razor ones, can only be viewed.
    /// </summary>
    public bool IsLiquid => FilePath?.EndsWith(".liquid", StringComparison.OrdinalIgnoreCase) == true;
}
