using System.Text.Json.Serialization;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Realtime;

/// <summary>
/// The payload broadcast to clients when media changes.
/// </summary>
/// <remarks>
/// Changes to this shape must be additive: clients ignore unknown properties, and older clients must keep
/// working against a newer server.
/// </remarks>
public sealed class MediaChangedEvent
{
    /// <summary>
    /// Gets or sets what happened: <c>fileUploaded</c>, <c>fileDeleted</c>, <c>fileMoved</c>,
    /// <c>fileCopied</c>, <c>directoryCreated</c> or <c>directoryDeleted</c>.
    /// </summary>
    [JsonPropertyName("action")]
    public string Action { get; set; }

    /// <summary>
    /// Gets or sets the affected path. For moves and copies this is the source.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; }

    /// <summary>
    /// Gets or sets the destination path, for moves and copies.
    /// </summary>
    [JsonPropertyName("newPath")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string NewPath { get; set; }

    /// <summary>
    /// Gets or sets the affected entry, shaped exactly as the <c>GetDirectoryContent</c> endpoint returns
    /// it, so clients can patch their list without a refetch. Null when the entry could not be resolved —
    /// clients then fall back to reloading the directory.
    /// </summary>
    [JsonPropertyName("item")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FileStoreEntryDto Item { get; set; }
}
