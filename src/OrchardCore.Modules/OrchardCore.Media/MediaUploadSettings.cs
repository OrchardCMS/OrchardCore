namespace OrchardCore.Media;

/// <summary>Optional tenant upload restrictions, bounded by the host's media configuration.</summary>
public sealed class MediaUploadSettings
{
    /// <summary>Gets or sets the tenant maximum file size in bytes; null inherits the host maximum.</summary>
    public long? MaxFileSize { get; set; }

    /// <summary>Gets or sets a subset of host-allowed and restricted extensions; null inherits the host lists and an empty array permits none.</summary>
    public string[] AllowedFileExtensions { get; set; }
}
