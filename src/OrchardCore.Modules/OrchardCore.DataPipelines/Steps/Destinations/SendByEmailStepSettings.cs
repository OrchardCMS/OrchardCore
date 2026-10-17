namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// The settings of a <see cref="SendByEmailStep"/>.
/// </summary>
public sealed class SendByEmailStepSettings
{
    /// <summary>
    /// The maximum size of the attachments of an email, in megabytes, by default.
    /// </summary>
    public const int DefaultMaxAttachmentSizeMegabytes = 10;

    /// <summary>
    /// Gets or sets the recipients, separated by commas or semicolons.
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// Gets or sets the recipients of a copy, separated by commas or semicolons.
    /// </summary>
    public string Cc { get; set; }

    /// <summary>
    /// Gets or sets the recipients of a blind copy, separated by commas or semicolons.
    /// </summary>
    public string Bcc { get; set; }

    /// <summary>
    /// Gets or sets the subject. It can use the placeholders of <see cref="DataPipelineTemplate"/> and
    /// <c>{FileNames}</c>, the names of the attached files.
    /// </summary>
    public string Subject { get; set; } = "{PipelineName} - {Date:yyyy-MM-dd}";

    /// <summary>
    /// Gets or sets the body. It can use the placeholders of <see cref="DataPipelineTemplate"/> and
    /// <c>{FileNames}</c>, the names of the attached files.
    /// </summary>
    public string Body { get; set; } = "The files of {PipelineName} are attached: {FileNames}.";

    /// <summary>
    /// Gets or sets a value indicating whether the body is HTML; otherwise it is plain text.
    /// </summary>
    public bool IsHtmlBody { get; set; }

    /// <summary>
    /// Gets or sets the maximum total size of the attachments, in megabytes. When the files are larger, the step fails.
    /// </summary>
    public int MaxAttachmentSizeMegabytes { get; set; } = DefaultMaxAttachmentSizeMegabytes;

    /// <summary>
    /// Gets or sets a value indicating whether an email is sent when there are no files to attach; by default, the
    /// step sends nothing.
    /// </summary>
    public bool SendWhenEmpty { get; set; }
}
