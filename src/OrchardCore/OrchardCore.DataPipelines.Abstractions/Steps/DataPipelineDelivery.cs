namespace OrchardCore.DataPipelines.Steps;

/// <summary>
/// Something a run delivered, such as a file saved to the media library or sent by email. Run histories list them.
/// </summary>
public sealed class DataPipelineDelivery
{
    /// <summary>
    /// Gets or sets the step that delivered it.
    /// </summary>
    public string StepId { get; set; }

    /// <summary>
    /// Gets or sets what was delivered, and where, as shown to users. It must not contain secrets.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets an optional link to what was delivered, such as the URL of a media file.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets when it was delivered, in UTC.
    /// </summary>
    public DateTime DeliveredUtc { get; set; } = DateTime.UtcNow;
}
