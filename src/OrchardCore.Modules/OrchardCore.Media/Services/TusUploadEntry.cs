namespace OrchardCore.Media.Services;

public sealed class TusUploadEntry
{
    /// <summary>
    /// The destination path within the media store (e.g. "images/photos").
    /// </summary>
    public string DestinationPath { get; set; }

    /// <summary>
    /// The normalized file name.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// The final media file path after the upload is completed and stored.
    /// Set by <c>OnFileCompleteAsync</c>.
    /// </summary>
    public string MediaFilePath { get; set; }
}
