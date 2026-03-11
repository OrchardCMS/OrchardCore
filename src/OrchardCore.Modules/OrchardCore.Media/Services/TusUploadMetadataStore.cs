using System.Collections.Concurrent;

namespace OrchardCore.Media.Services;

/// <summary>
/// In-memory store that maps TUS upload IDs to their destination metadata.
/// Entries are added when an upload is created and removed after the client
/// retrieves the completed file info.
/// </summary>
public sealed class TusUploadMetadataStore
{
    private readonly ConcurrentDictionary<string, TusUploadEntry> _entries = new();

    public void Set(string uploadId, TusUploadEntry entry) =>
        _entries[uploadId] = entry;

    public TusUploadEntry Get(string uploadId) =>
        _entries.TryGetValue(uploadId, out var entry) ? entry : null;

    public TusUploadEntry Remove(string uploadId) =>
        _entries.TryRemove(uploadId, out var entry) ? entry : null;
}

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
