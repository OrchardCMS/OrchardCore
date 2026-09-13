namespace OrchardCore.Indexing.Core.Operations;

/// <summary>A persisted tenant-local record of an indexing lifecycle request.</summary>
public sealed class IndexOperation
{
    /// <summary>Gets or sets the opaque operation identifier.</summary>
    public string OperationId { get; set; }
    /// <summary>Gets or sets the target index profile identifier.</summary>
    public string IndexId { get; set; }
    /// <summary>Gets or sets the requested lifecycle action.</summary>
    public IndexLifecycleAction Action { get; set; }
    /// <summary>Gets or sets the operation state.</summary>
    public IndexOperationState State { get; set; }
    /// <summary>Gets or sets the creation time in UTC.</summary>
    public DateTime CreatedUtc { get; set; }
    /// <summary>Gets or sets the latest state-transition time in UTC.</summary>
    public DateTime UpdatedUtc { get; set; }
    /// <summary>Gets or sets the processing outcome, when known.</summary>
    public IndexProcessingStatus? Outcome { get; set; }
    /// <summary>Gets or sets the last confirmed provider cursor.</summary>
    public long? LastTaskId { get; set; }
}

/// <summary>The persisted state of a lifecycle operation.</summary>
public enum IndexOperationState
{
    /// <summary>The operation was recorded but has not started.</summary>
    Pending,
    /// <summary>The operation is executing.</summary>
    Running,
    /// <summary>The shared processor confirmed completion.</summary>
    Completed,
    /// <summary>The operation did not complete successfully.</summary>
    Failed,
    /// <summary>Completion is not confirmed after the observation deadline; work may still be running.</summary>
    Uncertain,
}
