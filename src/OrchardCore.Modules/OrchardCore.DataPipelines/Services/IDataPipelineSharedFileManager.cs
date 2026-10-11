using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Shares the files of runs with users through download links.
/// </summary>
public interface IDataPipelineSharedFileManager
{
    /// <summary>
    /// Keeps a copy of a file, and creates a download link for it that works for the recipients only.
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="request">Who to share it with, and for how long.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns>The shared file and its link.</returns>
    Task<DataPipelineSharedFileResult> ShareAsync(DataPipelineFile file, DataPipelineSharedFileRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// What a file is shared with, and for how long.
/// </summary>
public sealed class DataPipelineSharedFileRequest
{
    /// <summary>
    /// Gets or sets the user names or emails of the recipients.
    /// </summary>
    public IReadOnlyList<string> Recipients { get; set; } = [];

    /// <summary>
    /// Gets or sets how long the link works.
    /// </summary>
    public TimeSpan Lifetime { get; set; } = TimeSpan.FromDays(7);

    /// <summary>
    /// Gets or sets the run that shares the file.
    /// </summary>
    public DataPipelineRunContext Run { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the recipients receive the link by email.
    /// </summary>
    public bool NotifyByEmail { get; set; }

    /// <summary>
    /// Gets or sets the subject of the email.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets a message added to the email.
    /// </summary>
    public string Message { get; set; }
}

/// <summary>
/// A shared file and its link.
/// </summary>
public sealed class DataPipelineSharedFileResult
{
    /// <summary>
    /// Gets or sets the shared file.
    /// </summary>
    public DataPipelineSharedFile SharedFile { get; set; }

    /// <summary>
    /// Gets or sets the download link. It holds the token, so it is only sent to the recipients.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the recipients who were notified by email.
    /// </summary>
    public IReadOnlyList<string> Notified { get; set; } = [];
}
