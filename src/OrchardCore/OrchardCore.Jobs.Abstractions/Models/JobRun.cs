using System.Text.Json.Nodes;
using OrchardCore.Entities;

namespace OrchardCore.Jobs.Models;

/// <summary>
/// A run of a job: what queued it, when it is due, which server executes it, how far it is, and how it ended.
/// Extensions keep their own data in <see cref="Entity.Properties"/>.
/// </summary>
public sealed class JobRun : Entity
{
    /// <summary>
    /// The name of the YesSql collection that stores the runs and the schedules.
    /// </summary>
    public const string Collection = "Jobs";

    /// <summary>
    /// The number of log entries a run keeps.
    /// </summary>
    public const int MaxLogEntries = 200;

    /// <summary>
    /// Gets or sets the document identifier.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the run.
    /// </summary>
    public string RunId { get; set; }

    /// <summary>
    /// Gets or sets the name of the job, as it is registered.
    /// </summary>
    public string JobName { get; set; }

    /// <summary>
    /// Gets or sets what this run is about, such as the name of the item it processes, shown next to the title of
    /// the job. It is optional.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets how far the run got.
    /// </summary>
    public JobRunStatus Status { get; set; }

    /// <summary>
    /// Gets or sets what started the run, one of the <see cref="JobTriggers"/> or a name of a module's own.
    /// </summary>
    public string Trigger { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who started the run, if any.
    /// </summary>
    public string TriggeredBy { get; set; }

    /// <summary>
    /// Gets or sets an identifier that links the run to what queued it, such as a workflow instance.
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the key that prevents the same work from being queued twice: while a run of the job with this
    /// key is queued, running or waiting, queuing another one returns that run.
    /// </summary>
    public string DeduplicationKey { get; set; }

    /// <summary>
    /// Gets or sets the key of the runs that execute one at a time: a run doesn't start while another run with
    /// this key is running, on any server.
    /// </summary>
    public string ConcurrencyKey { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the run that queued this run as one of its child runs, if any.
    /// </summary>
    public string ParentRunId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the schedule that created the run, if any.
    /// </summary>
    public string ScheduleId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the only server that may execute the run, for a run that needs something only
    /// that server has, such as a local file. When <see langword="null"/>, any server executes the run.
    /// </summary>
    public string RequiredNodeId { get; set; }

    /// <summary>
    /// Gets or sets the parameters of the run.
    /// </summary>
    public JsonObject Parameters { get; set; }

    /// <summary>
    /// Gets or sets the result the job set, once the run succeeded.
    /// </summary>
    public JsonNode Result { get; set; }

    /// <summary>
    /// Gets or sets when the run was queued, in UTC.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the run may start, in UTC. A queued run whose due date is in the future is scheduled.
    /// </summary>
    public DateTime DueUtc { get; set; }

    /// <summary>
    /// Gets or sets when the last attempt started, in UTC.
    /// </summary>
    public DateTime? StartedUtc { get; set; }

    /// <summary>
    /// Gets or sets when the run ended, in UTC.
    /// </summary>
    public DateTime? CompletedUtc { get; set; }

    /// <summary>
    /// Gets or sets how long the last attempt took, in milliseconds, once it ended. The dates of a run are stored
    /// to the second, so this is what tells how long a short run took.
    /// </summary>
    public long? DurationMilliseconds { get; set; }

    /// <summary>
    /// Gets or sets when the server executing the run last reported, in UTC. A running run that stops reporting is
    /// considered abandoned.
    /// </summary>
    public DateTime? HeartbeatUtc { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the server that executes the run, or executed its last attempt.
    /// </summary>
    public string NodeId { get; set; }

    /// <summary>
    /// Gets or sets the number of attempts that started, the first one being 1.
    /// </summary>
    public int Attempt { get; set; }

    /// <summary>
    /// Gets or sets the number of attempts the run may make. With 1, a run that fails or whose server stops is not
    /// executed again.
    /// </summary>
    public int MaxAttempts { get; set; } = 1;

    /// <summary>
    /// Gets or sets the progress the job reported, from 0 to 100, or <see langword="null"/> when it is unknown.
    /// </summary>
    public int? Progress { get; set; }

    /// <summary>
    /// Gets or sets what the job reported it is doing.
    /// </summary>
    public string ProgressMessage { get; set; }

    /// <summary>
    /// Gets or sets why the run failed or was cancelled.
    /// </summary>
    public string Error { get; set; }

    /// <summary>
    /// Gets or sets the details of the exception that failed the run, such as its stack trace.
    /// </summary>
    public string ErrorDetails { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a cancellation was requested. A running run stops at the next
    /// heartbeat of its server.
    /// </summary>
    public bool CancellationRequested { get; set; }

    /// <summary>
    /// Gets or sets the messages recorded during the run, the oldest first, up to <see cref="MaxLogEntries"/>.
    /// </summary>
    public List<JobLogEntry> Log { get; set; } = [];

    /// <summary>
    /// Gets how long the last attempt of the run took, once it ended.
    /// </summary>
    /// <returns>The duration, or <see langword="null"/> when the run didn't start or didn't end.</returns>
    public TimeSpan? GetDuration()
    {
        if (DurationMilliseconds is { } milliseconds)
        {
            return TimeSpan.FromMilliseconds(milliseconds);
        }

        return StartedUtc is { } started && CompletedUtc is { } completed ? completed - started : null;
    }

    /// <summary>
    /// Gets a value indicating whether the run ended.
    /// </summary>
    public bool IsCompleted
    {
        get
        {
            return Status is JobRunStatus.Succeeded or JobRunStatus.Failed or JobRunStatus.Canceled;
        }
    }
}
