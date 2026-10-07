using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Tells the open designers and instance pages that a workflow type or an instance changed. The default
/// implementation does nothing; the <c>OrchardCore.Workflows.SignalR</c> feature sends the changes to the
/// browsers once the request's changes are committed.
/// </summary>
public interface IWorkflowDesignerNotifier
{
    /// <summary>
    /// The draft of a workflow type changed, was published or was discarded.
    /// </summary>
    Task WorkflowTypeChangedAsync(WorkflowTypeChange change);

    /// <summary>
    /// A workflow instance was saved after running.
    /// </summary>
    Task InstanceChangedAsync(WorkflowInstanceChange change);
}

/// <summary>
/// What happened to a workflow type.
/// </summary>
public enum WorkflowTypeChangeKind
{
    /// <summary>
    /// The draft changed.
    /// </summary>
    DraftChanged,

    /// <summary>
    /// The draft was published.
    /// </summary>
    Published,

    /// <summary>
    /// The draft was discarded.
    /// </summary>
    DraftDiscarded,
}

/// <summary>
/// A change of a workflow type.
/// </summary>
public sealed class WorkflowTypeChange
{
    public WorkflowTypeChangeKind Kind { get; init; }

    /// <summary>
    /// The <see cref="WorkflowType.WorkflowTypeId"/>.
    /// </summary>
    public string WorkflowTypeId { get; init; }

    /// <summary>
    /// The revision of the draft after the change, or 0 when it was published or discarded.
    /// </summary>
    public int Revision { get; init; }

    /// <summary>
    /// The version a publish created, if any.
    /// </summary>
    public string VersionId { get; init; }

    /// <summary>
    /// The user who made the change.
    /// </summary>
    public string UserId { get; init; }

    /// <summary>
    /// The name of the user who made the change.
    /// </summary>
    public string UserName { get; init; }
}

/// <summary>
/// A change of a workflow instance.
/// </summary>
public sealed class WorkflowInstanceChange
{
    /// <summary>
    /// The <see cref="Workflow.WorkflowId"/>.
    /// </summary>
    public string WorkflowId { get; init; }

    /// <summary>
    /// The <see cref="Workflow.WorkflowTypeId"/>.
    /// </summary>
    public string WorkflowTypeId { get; init; }

    /// <summary>
    /// The status of the instance after the change.
    /// </summary>
    public WorkflowStatus Status { get; init; }

    /// <summary>
    /// Whether the instance was deleted (a finished instance of a type that deletes them).
    /// </summary>
    public bool IsDeleted { get; init; }
}
