using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes;

/// <summary>
/// Data transfer object for altering a content type part.
/// </summary>
public class AlterTypePartContext
{
    /// <summary>
    /// Gets or sets the name of the content type.
    /// </summary>
    public string TypeName { get; set; }

    /// <summary>
    /// Gets or sets the content part definition.
    /// </summary>
    public ContentPartDefinition PartDefinition { get; set; }

    /// <summary>
    /// Gets or sets the name of the part instance on the type.
    /// </summary>
    public string PartName { get; set; }

    /// <summary>
    /// Gets or sets the display name of the part.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the description of the part.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the editor for the part.
    /// </summary>
    public string Editor { get; set; }

    /// <summary>
    /// Gets or sets the display mode for the part.
    /// </summary>
    public string DisplayMode { get; set; }
}
