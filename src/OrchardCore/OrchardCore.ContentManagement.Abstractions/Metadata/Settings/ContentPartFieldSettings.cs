namespace OrchardCore.ContentManagement.Metadata.Settings;

public class ContentPartFieldSettings
{
    /// <summary>
    /// Gets or sets the displayed name of the part field.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the description of the part field.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the editor of the part field.
    /// </summary>
    public string Editor { get; set; }

    /// <summary>
    /// Gets or sets the display mode of the part field.
    /// </summary>
    public string DisplayMode { get; set; }

    /// <summary>
    /// Gets or sets the position of the part field in the editor.
    /// </summary>
    public string Position { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the name of the field is displayed next to its value.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="true"/>, so a field defined without this setting displays its name.
    /// </remarks>
    public bool DisplayFieldName { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the field is displayed when it has no value.
    /// </summary>
    /// <remarks>
    /// Defaults to <see langword="false"/>, so a field without a value is not displayed.
    /// </remarks>
    public bool AlwaysDisplay { get; set; }
}
