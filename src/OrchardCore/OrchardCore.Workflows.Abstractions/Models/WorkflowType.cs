using OrchardCore.Entities;

namespace OrchardCore.Workflows.Models;

/// <summary>
/// Represents a workflow type.
/// </summary>
public class WorkflowType : Entity
{
    public long Id { get; set; }

    /// <summary>
    /// A unique identifier for this workflow type.
    /// </summary>
    public string WorkflowTypeId { get; set; }

    /// <summary>
    /// The <see cref="WorkflowTypeVersion.VersionId"/> of the version this definition was saved as: the version
    /// new instances start on. <see cref="Services.IWorkflowTypeStore.SaveAsync"/> sets it.
    /// </summary>
    public string VersionId { get; set; }

    /// <summary>
    /// The name of this workflow type.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Whether this workflow definition is enabled or not.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Controls whether this workflow can spawn one or multiple instances.
    /// </summary>
    public bool IsSingleton { get; set; }

    /// <summary>
    /// The timeout in milliseconds to acquire a lock before resuming a given workflow instance of this type.
    /// </summary>
    public int LockTimeout { get; set; }

    /// <summary>
    /// The expiration in milliseconds of the lock acquired before resuming a workflow instance of this type.
    /// </summary>
    public int LockExpiration { get; set; }

    /// <summary>
    /// Controls whether workflow instances will be deleted upon completion.
    /// </summary>
    public bool DeleteFinishedWorkflows { get; set; }

    /// <summary>
    /// Whether other workflows can run this one as an activity (Execute Workflow), with its input and output variables.
    /// </summary>
    public bool IsActivity { get; set; }

    /// <summary>
    /// How the engine follows an outcome that has several transitions: only the first one (the default), or all of them.
    /// </summary>
    public WorkflowBranchingMode BranchingMode { get; set; }

    /// <summary>
    /// Whether a script error faults the instance at the activity that ran the script. Otherwise, the run goes on with what the script fell back to, and the error is recorded in the journal.
    /// </summary>
    public bool FaultOnScriptErrors { get; set; }

    /// <summary>
    /// A complete list of all activities that are part of this workflow.
    /// </summary>
    public IList<ActivityRecord> Activities { get; set; } = [];

    /// <summary>
    /// A complete list of the transitions between the activities on this workflow.
    /// </summary>
    public IList<Transition> Transitions { get; set; } = [];

    /// <summary>
    /// The variables this workflow declares.
    /// </summary>
    public IList<WorkflowVariableDefinition> Variables { get; set; } = [];
}
