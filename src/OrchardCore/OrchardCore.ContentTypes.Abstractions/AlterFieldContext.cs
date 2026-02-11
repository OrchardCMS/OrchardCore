namespace OrchardCore.ContentTypes;

/// <summary>
/// Data transfer object for altering a content field.
/// </summary>
public class AlterFieldContext
{
    /// <summary>
    /// Gets or sets the name of the part containing the field.
    /// </summary>
    public string PartName { get; set; }

    /// <summary>
    /// Gets or sets the technical name of the field.
    /// </summary>
    public string FieldName { get; set; }

    /// <summary>
    /// Gets or sets the display name of the field.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the editor for the field.
    /// </summary>
    public string Editor { get; set; }

    /// <summary>
    /// Gets or sets the display mode for the field.
    /// </summary>
    public string DisplayMode { get; set; }
}
