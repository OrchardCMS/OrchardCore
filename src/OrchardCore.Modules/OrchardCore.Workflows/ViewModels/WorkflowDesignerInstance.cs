using System.Text.Json.Nodes;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels;

/// <summary>
/// The workflow instance shown by the read-only instance viewer.
/// </summary>
public sealed class WorkflowDesignerInstance
{
    /// <summary>
    /// The document identifier of the instance.
    /// </summary>
    public long Id { get; init; }

    /// <summary>
    /// The <see cref="Workflow.WorkflowId"/>.
    /// </summary>
    public string WorkflowId { get; init; }

    /// <summary>
    /// The name of the instance's <see cref="WorkflowStatus"/>.
    /// </summary>
    public string Status { get; init; }

    /// <summary>
    /// The activities the instance waits on.
    /// </summary>
    public IReadOnlyList<string> BlockingActivityIds { get; init; } = [];

    /// <summary>
    /// The stored values of the declared variables, by name.
    /// </summary>
    public IReadOnlyDictionary<string, JsonNode> VariableValues { get; init; } = new Dictionary<string, JsonNode>();

    /// <summary>
    /// The error of a faulted instance.
    /// </summary>
    public string FaultMessage { get; init; }

    /// <summary>
    /// The activity whose execution faulted the instance, from its journal.
    /// </summary>
    public string FaultedActivityId { get; init; }

    /// <summary>
    /// The most recent records of the instance's journal, oldest first.
    /// </summary>
    public IReadOnlyList<WorkflowDesignerJournalRecord> Journal { get; init; } = [];

    /// <summary>
    /// How many times each activity ran, by activity id, according to the journal.
    /// </summary>
    public IReadOnlyDictionary<string, int> ExecutedActivityCounts { get; init; } = new Dictionary<string, int>();

    /// <summary>
    /// How many times each transition was taken, by transition key (<c>source:outcome:destination</c>).
    /// </summary>
    public IReadOnlyDictionary<string, int> ExecutedTransitionCounts { get; init; } = new Dictionary<string, int>();
}

/// <summary>
/// A record of an instance's journal, as shown by the viewer.
/// </summary>
public sealed class WorkflowDesignerJournalRecord
{
    public int Sequence { get; init; }

    public string ActivityId { get; init; }

    public string ActivityName { get; init; }

    public string ActivityTitle { get; init; }

    public bool IsResume { get; init; }

    /// <summary>
    /// The name of the <see cref="WorkflowExecutionRecordStatus"/>.
    /// </summary>
    public string Status { get; init; }

    public IReadOnlyList<string> Outcomes { get; init; } = [];

    public DateTime StartedUtc { get; init; }

    public DateTime CompletedUtc { get; init; }

    /// <summary>
    /// How long the execution took, in milliseconds.
    /// </summary>
    public double DurationMilliseconds { get; init; }

    public string Error { get; init; }

    public static WorkflowDesignerJournalRecord From(WorkflowExecutionRecord record)
        => new()
        {
            Sequence = record.Sequence,
            ActivityId = record.ActivityId,
            ActivityName = record.ActivityName,
            ActivityTitle = record.ActivityTitle,
            IsResume = record.IsResume,
            Status = record.Status.ToString(),
            Outcomes = record.Outcomes?.ToList() ?? [],
            StartedUtc = record.StartedUtc,
            CompletedUtc = record.CompletedUtc,
            DurationMilliseconds = record.DurationMilliseconds,
            Error = record.Error,
        };
}
