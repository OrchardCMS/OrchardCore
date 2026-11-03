namespace OrchardCore.Workflows.Models;

/// <summary>
/// The result of an operation on a <see cref="WorkflowTypeDraft"/>.
/// </summary>
public sealed class WorkflowTypeDraftResult
{
    /// <summary>
    /// The outcome of the operation.
    /// </summary>
    public WorkflowTypeDraftStatus Status { get; init; }

    /// <summary>
    /// The draft after the operation when it succeeded, or the current draft when it conflicted.
    /// </summary>
    public WorkflowTypeDraft Draft { get; init; }

    /// <summary>
    /// The new revision when the operation succeeded, or the current revision when it conflicted.
    /// </summary>
    public int Revision { get; init; }

    /// <summary>
    /// The name of the user who last changed the draft, set when the operation conflicted.
    /// </summary>
    public string ModifiedByUserName { get; init; }

    /// <summary>
    /// When the draft was last changed, set when the operation conflicted.
    /// </summary>
    public DateTime? ModifiedUtc { get; init; }

    /// <summary>
    /// The added or updated activity, for the operations that change one activity.
    /// </summary>
    public ActivityRecord Activity { get; init; }

    /// <summary>
    /// The transitions removed because the updated activity no longer produces their outcome.
    /// </summary>
    public IReadOnlyList<Transition> RemovedTransitions { get; init; } = [];

    /// <summary>
    /// The validation issues of the draft, set by the operations that validate it.
    /// </summary>
    public IReadOnlyList<WorkflowDesignIssue> Issues { get; init; } = [];

    /// <summary>
    /// The live workflow type, set when a draft was published.
    /// </summary>
    public WorkflowType WorkflowType { get; init; }

    /// <summary>
    /// Whether the operation succeeded.
    /// </summary>
    public bool Succeeded => Status == WorkflowTypeDraftStatus.Succeeded;
}

/// <summary>
/// The outcome of an operation on a <see cref="WorkflowTypeDraft"/>.
/// </summary>
public enum WorkflowTypeDraftStatus
{
    /// <summary>
    /// The operation was applied.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The expected revision doesn't match the current one; another change was saved in between.
    /// </summary>
    Conflict,

    /// <summary>
    /// The workflow type, or the activity the operation is about, doesn't exist.
    /// </summary>
    NotFound,

    /// <summary>
    /// The operation was rejected, for example an unknown activity type, or publishing a draft that has errors.
    /// </summary>
    Invalid,
}
