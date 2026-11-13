using OrchardCore.Jobs.Models;
using YesSql.Indexes;

namespace OrchardCore.Jobs.Indexes;

/// <summary>
/// Indexes the runs, to find the ones that are due, abandoned or old, and to list them.
/// </summary>
public sealed class JobRunIndex : MapIndex
{
    public const int MaxIdLength = 26;

    public const int MaxJobNameLength = 100;

    public const int MaxKeyLength = 255;

    public long DocumentId { get; set; }

    public string RunId { get; set; }

    public string JobName { get; set; }

    public string Status { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime DueUtc { get; set; }

    public DateTime? CompletedUtc { get; set; }

    public DateTime? HeartbeatUtc { get; set; }

    public string NodeId { get; set; }

    public string RequiredNodeId { get; set; }

    public string CorrelationId { get; set; }

    public string DeduplicationKey { get; set; }

    public string ConcurrencyKey { get; set; }

    /// <summary>
    /// Whether the run replaces the runs with the same concurrency key that were queued before it.
    /// </summary>
    public bool CancelInProgress { get; set; }

    public string ParentRunId { get; set; }

    public string ScheduleId { get; set; }
}

/// <summary>
/// Indexes the schedules, to find the ones that are due.
/// </summary>
public sealed class JobScheduleIndex : MapIndex
{
    public long DocumentId { get; set; }

    public string ScheduleId { get; set; }

    public string JobName { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime? NextOccurrenceUtc { get; set; }
}

public sealed class JobRunIndexProvider : IndexProvider<JobRun>
{
    public JobRunIndexProvider() => CollectionName = JobRun.Collection;

    public override void Describe(DescribeContext<JobRun> context)
    {
        // A run is a document with a single record here, so a query that only joins indexes with a single record per document isn't
        // grouped by document, and its count reads this table without joining the document table.
        context.For<JobRunIndex>()
            .ExclusiveToDocumentType()
            .SingleRecordPerDocument()
            .Map(run => new JobRunIndex
            {
                RunId = run.RunId,
                JobName = Truncate(run.JobName, JobRunIndex.MaxJobNameLength),
                Status = run.Status.ToString(),
                CreatedUtc = run.CreatedUtc,
                DueUtc = run.DueUtc,
                CompletedUtc = run.CompletedUtc,
                HeartbeatUtc = run.HeartbeatUtc,
                NodeId = Truncate(run.NodeId, JobRunIndex.MaxKeyLength),
                RequiredNodeId = Truncate(run.RequiredNodeId, JobRunIndex.MaxKeyLength),
                CorrelationId = Truncate(run.CorrelationId, JobRunIndex.MaxKeyLength),
                DeduplicationKey = Truncate(run.DeduplicationKey, JobRunIndex.MaxKeyLength),
                ConcurrencyKey = Truncate(run.ConcurrencyKey, JobRunIndex.MaxKeyLength),
                CancelInProgress = run.CancelInProgress,
                ParentRunId = run.ParentRunId,
                ScheduleId = Truncate(run.ScheduleId, JobRunIndex.MaxKeyLength),
            });
    }

    internal static string Truncate(string value, int length)
    {
        return value?.Length > length ? value[..length] : value;
    }
}

public sealed class JobScheduleIndexProvider : IndexProvider<JobSchedule>
{
    public JobScheduleIndexProvider() => CollectionName = JobRun.Collection;

    public override void Describe(DescribeContext<JobSchedule> context)
    {
        context.For<JobScheduleIndex>()
            .ExclusiveToDocumentType()
            .SingleRecordPerDocument()
            .Map(schedule => new JobScheduleIndex
            {
                ScheduleId = JobRunIndexProvider.Truncate(schedule.ScheduleId, JobRunIndex.MaxKeyLength),
                JobName = JobRunIndexProvider.Truncate(schedule.JobName, JobRunIndex.MaxJobNameLength),
                IsEnabled = schedule.IsEnabled,
                NextOccurrenceUtc = schedule.NextOccurrenceUtc,
            });
    }
}
