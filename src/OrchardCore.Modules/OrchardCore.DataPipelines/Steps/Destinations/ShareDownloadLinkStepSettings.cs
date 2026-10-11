namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="ShareDownloadLinkStep"/>.
/// </summary>
public sealed class ShareDownloadLinkStepSettings
{
    /// <summary>
    /// Gets or sets the user names or emails of the users the files are shared with.
    /// </summary>
    public List<string> Recipients { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of days a link works.
    /// </summary>
    public int ExpiresAfterDays { get; set; } = 7;

    /// <summary>
    /// Gets or sets a value indicating whether the recipients receive the link by email.
    /// </summary>
    public bool NotifyByEmail { get; set; } = true;

    /// <summary>
    /// Gets or sets the subject of the email. It can use the placeholders of <see cref="DataPipelineTemplate"/>.
    /// </summary>
    public string Subject { get; set; } = "{PipelineName}: a file is ready";

    /// <summary>
    /// Gets or sets a message added to the email.
    /// </summary>
    public string Message { get; set; }
}
