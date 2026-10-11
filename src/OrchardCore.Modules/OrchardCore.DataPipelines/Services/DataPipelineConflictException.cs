using OrchardCore.DataPipelines.Models;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Thrown when a draft is saved on a revision that is no longer the current one, because someone else saved it
/// meanwhile.
/// </summary>
public sealed class DataPipelineConflictException : Exception
{
    public DataPipelineConflictException()
    {
    }

    public DataPipelineConflictException(string message)
        : base(message)
    {
    }

    public DataPipelineConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public DataPipelineConflictException(DataPipeline pipeline)
        : base("The draft was changed since it was loaded.")
    {
        ArgumentNullException.ThrowIfNull(pipeline);

        CurrentRevision = pipeline.Revision;
        ModifiedBy = pipeline.DraftModifiedBy;
        ModifiedUtc = pipeline.DraftModifiedUtc;
    }

    /// <summary>
    /// Gets the current revision.
    /// </summary>
    public int CurrentRevision { get; }

    /// <summary>
    /// Gets the name of the user who last saved the draft.
    /// </summary>
    public string ModifiedBy { get; }

    /// <summary>
    /// Gets when the draft was last saved, in UTC.
    /// </summary>
    public DateTime? ModifiedUtc { get; }
}
