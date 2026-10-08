namespace OrchardCore.Workflows.Models;

/// <summary>
/// A record of the workflow journal: one execution of an activity by a workflow instance. Records are stored in
/// their own collection (<see cref="Collection"/>).
/// </summary>
public sealed class WorkflowExecutionRecord
{
    /// <summary>
    /// The YesSql collection of the journal.
    /// </summary>
    public const string Collection = "WorkflowJournal";

    /// <summary>
    /// The document id.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The <see cref="Workflow.WorkflowId"/> of the instance.
    /// </summary>
    public string WorkflowId { get; set; }

    /// <summary>
    /// The <see cref="WorkflowType.WorkflowTypeId"/> of the instance.
    /// </summary>
    public string WorkflowTypeId { get; set; }

    /// <summary>
    /// The position of the record in the journal of the instance, from 1.
    /// </summary>
    public int Sequence { get; set; }

    /// <summary>
    /// The <see cref="ActivityRecord.ActivityId"/>.
    /// </summary>
    public string ActivityId { get; set; }

    /// <summary>
    /// The name of the activity type.
    /// </summary>
    public string ActivityName { get; set; }

    /// <summary>
    /// The title of the activity, when it has one.
    /// </summary>
    public string ActivityTitle { get; set; }

    /// <summary>
    /// Whether the activity was resumed (an event that the instance waited on) rather than executed.
    /// </summary>
    public bool IsResume { get; set; }

    /// <summary>
    /// How the execution ended.
    /// </summary>
    public WorkflowExecutionRecordStatus Status { get; set; }

    /// <summary>
    /// The outcomes the activity produced.
    /// </summary>
    public IList<string> Outcomes { get; set; } = [];

    /// <summary>
    /// When the execution started.
    /// </summary>
    public DateTime StartedUtc { get; set; }

    /// <summary>
    /// When the execution ended.
    /// </summary>
    public DateTime CompletedUtc { get; set; }

    /// <summary>
    /// How long the execution took, in milliseconds. Dates are stored to the second, so the duration is stored on
    /// its own.
    /// </summary>
    public double DurationMilliseconds { get; set; }

    /// <summary>
    /// The error message, when the execution faulted.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// What the activity evaluated, set and changed, when its workflow records activity data
    /// (<see cref="WorkflowType.RecordActivityData"/>), or <see langword="null"/>.
    /// </summary>
    public WorkflowExecutionData Data { get; set; }
}

/// <summary>
/// How an activity execution ended.
/// </summary>
public enum WorkflowExecutionRecordStatus
{
    /// <summary>
    /// The activity produced its outcomes.
    /// </summary>
    Completed,

    /// <summary>
    /// The activity waits for an event; the instance is halted on it.
    /// </summary>
    Halted,

    /// <summary>
    /// The activity threw, which faulted the instance.
    /// </summary>
    Faulted,
}
