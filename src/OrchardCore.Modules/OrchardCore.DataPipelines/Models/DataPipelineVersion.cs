namespace OrchardCore.DataPipelines.Models;

/// <summary>
/// A published version of a pipeline, kept so it can be viewed, compared and restored, and so a run shows the
/// definition it executed.
/// </summary>
public sealed class DataPipelineVersion
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the version.
    /// </summary>
    public string VersionId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the pipeline.
    /// </summary>
    public string PipelineId { get; set; }

    /// <summary>
    /// Gets or sets the number of the version: 1 for the first publish, then 2, and so on.
    /// </summary>
    public int Number { get; set; }

    /// <summary>
    /// Gets or sets the name of the pipeline when the version was published.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the published definition.
    /// </summary>
    public DataPipelineDefinition Definition { get; set; }

    /// <summary>
    /// Gets or sets when the version was published, in UTC.
    /// </summary>
    public DateTime PublishedUtc { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who published the version.
    /// </summary>
    public string PublishedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who published the version.
    /// </summary>
    public string PublishedByUserId { get; set; }
}
