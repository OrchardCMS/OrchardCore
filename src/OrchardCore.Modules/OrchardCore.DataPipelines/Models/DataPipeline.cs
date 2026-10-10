namespace OrchardCore.DataPipelines.Models;

/// <summary>
/// A data pipeline: its draft, which the designer edits, and its published definition, which runs execute.
/// </summary>
public sealed class DataPipeline
{
    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the pipeline.
    /// </summary>
    public string PipelineId { get; set; }

    /// <summary>
    /// Gets or sets the name of the pipeline.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the pipeline.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the pipeline can run.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the draft the designer edits, or <see langword="null"/> when the pipeline has no unpublished
    /// changes.
    /// </summary>
    public DataPipelineDefinition Draft { get; set; }

    /// <summary>
    /// Gets or sets the number of times the draft was saved. A save that doesn't send the current revision is
    /// rejected, so two users don't overwrite each other's changes.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// Gets or sets when the draft was last changed, in UTC.
    /// </summary>
    public DateTime? DraftModifiedUtc { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who last changed the draft.
    /// </summary>
    public string DraftModifiedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last changed the draft.
    /// </summary>
    public string DraftModifiedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the published definition, or <see langword="null"/> when the pipeline was never published.
    /// </summary>
    public DataPipelineDefinition Published { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the published version.
    /// </summary>
    public string PublishedVersionId { get; set; }

    /// <summary>
    /// Gets or sets the number of the published version.
    /// </summary>
    public int PublishedVersionNumber { get; set; }

    /// <summary>
    /// Gets or sets when the pipeline was last published, in UTC.
    /// </summary>
    public DateTime? PublishedUtc { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who last published the pipeline.
    /// </summary>
    public string PublishedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last published the pipeline. Runs read and write data with this
    /// user's access: publishing vouches for what the pipeline does.
    /// </summary>
    public string PublishedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the steps recently removed from the draft, so the designer can undo their removal. The most
    /// recent last, up to 50.
    /// </summary>
    public List<DataPipelineStep> RemovedSteps { get; set; } = [];

    /// <summary>
    /// Gets or sets when the pipeline was created, in UTC.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who created the pipeline.
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets when the pipeline was last changed, in UTC.
    /// </summary>
    public DateTime ModifiedUtc { get; set; }

    /// <summary>
    /// Gets the definition the designer edits: the draft, or else the published definition, or else an empty one.
    /// </summary>
    /// <returns>The definition.</returns>
    public DataPipelineDefinition GetEditableDefinition()
        => Draft ?? Published ?? new DataPipelineDefinition();
}
