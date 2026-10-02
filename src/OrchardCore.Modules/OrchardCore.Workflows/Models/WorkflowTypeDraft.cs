namespace OrchardCore.Workflows.Models;

/// <summary>
/// A server-side working copy of a <see cref="WorkflowType"/> that the designer saves into. A draft never
/// executes; publishing it applies its content to the live workflow type.
/// </summary>
public sealed class WorkflowTypeDraft
{
    /// <summary>
    /// The document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The <see cref="WorkflowType.WorkflowTypeId"/> of the workflow type this draft belongs to.
    /// </summary>
    public string WorkflowTypeId { get; set; }

    /// <summary>
    /// A number that increments on every change, used to detect concurrent edits.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// When the draft was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// When the draft was last changed.
    /// </summary>
    public DateTime ModifiedUtc { get; set; }

    /// <summary>
    /// The identifier of the user who last changed the draft.
    /// </summary>
    public string ModifiedByUserId { get; set; }

    /// <summary>
    /// The name of the user who last changed the draft.
    /// </summary>
    public string ModifiedByUserName { get; set; }

    /// <summary>
    /// The draft value of <see cref="WorkflowType.Name"/>.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The draft value of <see cref="WorkflowType.IsEnabled"/>.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// The draft value of <see cref="WorkflowType.IsSingleton"/>.
    /// </summary>
    public bool IsSingleton { get; set; }

    /// <summary>
    /// The draft value of <see cref="WorkflowType.LockTimeout"/>.
    /// </summary>
    public int LockTimeout { get; set; }

    /// <summary>
    /// The draft value of <see cref="WorkflowType.LockExpiration"/>.
    /// </summary>
    public int LockExpiration { get; set; }

    /// <summary>
    /// The draft value of <see cref="WorkflowType.DeleteFinishedWorkflows"/>.
    /// </summary>
    public bool DeleteFinishedWorkflows { get; set; }

    /// <summary>
    /// The draft activities.
    /// </summary>
    public IList<ActivityRecord> Activities { get; set; } = [];

    /// <summary>
    /// The draft transitions.
    /// </summary>
    public IList<Transition> Transitions { get; set; } = [];

    /// <summary>
    /// The activities removed from the draft, most recent last, so that undoing a removal in the designer can
    /// restore them with their properties. They are never published.
    /// </summary>
    public IList<ActivityRecord> RemovedActivities { get; set; } = [];
}
