namespace OrchardCore.Workflows.Models;

/// <summary>
/// An immutable snapshot of a <see cref="WorkflowType"/>, created each time its definition changes (for
/// example when a draft is published). Instances run on the version they started on.
/// </summary>
/// <remarks>
/// A version holds what affects execution: the activities, the transitions, the variables and the execution settings.
/// <see cref="WorkflowType.Name"/> and <see cref="WorkflowType.IsEnabled"/> belong to the workflow type only;
/// changing them doesn't create a version.
/// </remarks>
public sealed class WorkflowTypeVersion
{
    /// <summary>
    /// The document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The <see cref="WorkflowType.WorkflowTypeId"/> of the workflow type this version belongs to.
    /// </summary>
    public string WorkflowTypeId { get; set; }

    /// <summary>
    /// A unique identifier for this version, stored in <see cref="WorkflowType.VersionId"/> and
    /// <see cref="Workflow.WorkflowTypeVersionId"/>.
    /// </summary>
    public string VersionId { get; set; }

    /// <summary>
    /// The number of this version within its workflow type, starting at 1.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// When this version was created.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// The identifier of the user who created this version, if any.
    /// </summary>
    public string CreatedByUserId { get; set; }

    /// <summary>
    /// The name of the user who created this version, if any.
    /// </summary>
    public string CreatedByUserName { get; set; }

    /// <summary>
    /// The name of the workflow type when this version was created.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The value of <see cref="WorkflowType.IsSingleton"/> in this version.
    /// </summary>
    public bool IsSingleton { get; set; }

    /// <summary>
    /// The value of <see cref="WorkflowType.LockTimeout"/> in this version.
    /// </summary>
    public int LockTimeout { get; set; }

    /// <summary>
    /// The value of <see cref="WorkflowType.LockExpiration"/> in this version.
    /// </summary>
    public int LockExpiration { get; set; }

    /// <summary>
    /// The value of <see cref="WorkflowType.DeleteFinishedWorkflows"/> in this version.
    /// </summary>
    public bool DeleteFinishedWorkflows { get; set; }

    /// <summary>
    /// The value of <see cref="WorkflowType.IsActivity"/> in this version.
    /// </summary>
    public bool IsActivity { get; set; }

    /// <summary>
    /// The value of <see cref="WorkflowType.BranchingMode"/> in this version.
    /// </summary>
    public WorkflowBranchingMode BranchingMode { get; set; }

    /// <summary>
    /// The value of <see cref="WorkflowType.FaultOnScriptErrors"/> in this version.
    /// </summary>
    public bool FaultOnScriptErrors { get; set; }

    /// <summary>
    /// The activities of this version.
    /// </summary>
    public IList<ActivityRecord> Activities { get; set; } = [];

    /// <summary>
    /// The transitions of this version.
    /// </summary>
    public IList<Transition> Transitions { get; set; } = [];

    /// <summary>
    /// The variables this version declares.
    /// </summary>
    public IList<WorkflowVariableDefinition> Variables { get; set; } = [];
}
