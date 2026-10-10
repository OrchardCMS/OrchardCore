namespace OrchardCore.DataPipelines.Models;

/// <summary>
/// A file a run shared with users through a download link. Only the SHA-256 hash of the link's token is stored, and
/// the link works for the recipients only, until it expires or is revoked.
/// </summary>
public sealed class DataPipelineSharedFile
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the shared file, which is part of its link.
    /// </summary>
    public string FileId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the pipeline that shared the file.
    /// </summary>
    public string PipelineId { get; set; }

    /// <summary>
    /// Gets or sets the name of the pipeline that shared the file.
    /// </summary>
    public string PipelineName { get; set; }

    /// <summary>
    /// Gets or sets the run that shared the file.
    /// </summary>
    public string RunId { get; set; }

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the media type of the file.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the size of the file, in bytes.
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// Gets or sets the SHA-256 hash of the link's token, as hexadecimal text.
    /// </summary>
    public string TokenHash { get; set; }

    /// <summary>
    /// Gets or sets the identifiers of the users who may download the file.
    /// </summary>
    public List<string> RecipientUserIds { get; set; } = [];

    /// <summary>
    /// Gets or sets the names of the users who may download the file, for display.
    /// </summary>
    public List<string> RecipientNames { get; set; } = [];

    /// <summary>
    /// Gets or sets when the file was shared, in UTC.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the link stops working, in UTC.
    /// </summary>
    public DateTime ExpiresUtc { get; set; }

    /// <summary>
    /// Gets or sets when the link was revoked, in UTC, if it was.
    /// </summary>
    public DateTime? RevokedUtc { get; set; }

    /// <summary>
    /// Gets or sets the number of times the file was downloaded.
    /// </summary>
    public int Downloads { get; set; }

    /// <summary>
    /// Gets or sets when the file was last downloaded, in UTC.
    /// </summary>
    public DateTime? LastDownloadedUtc { get; set; }

    /// <summary>
    /// Tells whether the link works at a given time.
    /// </summary>
    /// <param name="utcNow">The current time, in UTC.</param>
    /// <returns><see langword="true"/> when the link is neither expired nor revoked.</returns>
    public bool IsActive(DateTime utcNow) => RevokedUtc is null && ExpiresUtc > utcNow;
}
