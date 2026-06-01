namespace OrchardCore.Media.Settings;

/// <summary>
/// Editor-specific settings for the Attached media field editor.
/// These settings are only applied when the media field uses the "Attached" editor.
/// </summary>
public class MediaFieldAttachedEditorSettings
{
    /// <summary>
    /// Gets or sets an optional folder path where all media uploaded via this field should be stored.
    /// When set, uploaded files are moved to this folder instead of the default
    /// <c>mediafields/{ContentType}/{ContentItemId}/</c> path. This allows the Secure Media feature
    /// to restrict access to the folder using per-folder permissions.
    /// </summary>
    public string Folder { get; set; }
}
