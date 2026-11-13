using Microsoft.Extensions.Options;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Jobs;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.DataPipelines.Jobs;

/// <summary>
/// Deletes the data pipeline runs older than <see cref="DataPipelineOptions.RunRetentionDays"/>, and queues again
/// the runs that no job executes. It runs every hour, once whatever the number of servers.
/// </summary>
public sealed class DataPipelineRunsCleanupJob : IJob
{
    /// <summary>
    /// The name of the job.
    /// </summary>
    public const string Name = "DataPipelineRunsCleanup";

    private const int BatchSize = 100;
    private const int MaxBatches = 50;

    private static readonly TimeSpan _queueDelay = TimeSpan.FromMinutes(5);

    private readonly ISession _session;
    private readonly IClock _clock;
    private readonly IJobManager _jobManager;
    private readonly DataPipelineOptions _options;

    public DataPipelineRunsCleanupJob(
        ISession session,
        IClock clock,
        IJobManager jobManager,
        IOptions<DataPipelineOptions> options)
    {
        _session = session;
        _clock = clock;
        _jobManager = jobManager;
        _options = options.Value;
    }

    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        // A run that was queued without a job, such as before an upgrade, is given one.
        var queuedBefore = now - _queueDelay;
        var queuedStatus = nameof(DataPipelineRunStatus.Queued);
        var queued = await _session.Query<DataPipelineRun, DataPipelineRunIndex>(index => index.Status == queuedStatus && index.QueuedUtc < queuedBefore)
            .OrderBy(index => index.QueuedUtc)
            .Take(BatchSize)
            .ListAsync(cancellationToken);

        var requeued = 0;

        foreach (var run in queued)
        {
            if (!string.IsNullOrEmpty(run.JobRunId) && await _jobManager.GetAsync(run.JobRunId) is not null)
            {
                continue;
            }

            await DataPipelineRunManager.EnqueueJobAsync(_jobManager, run);
            await _session.SaveAsync(run, cancellationToken: cancellationToken);
            requeued++;
        }

        if (requeued > 0)
        {
            context.LogInformation($"{requeued} runs that no job executed were queued again.");
        }

        if (_options.RunRetentionDays <= 0)
        {
            return;
        }

        var completedBefore = now.AddDays(-_options.RunRetentionDays);
        var deleted = 0;

        for (var batch = 0; batch < MaxBatches && !cancellationToken.IsCancellationRequested; batch++)
        {
            var old = (await _session.Query<DataPipelineRun, DataPipelineRunIndex>(index => index.CompletedUtc != null && index.CompletedUtc < completedBefore)
                .Take(BatchSize)
                .ListAsync(cancellationToken)).ToList();

            if (old.Count == 0)
            {
                break;
            }

            foreach (var run in old)
            {
                _session.Delete(run);
            }

            // Each batch is committed, so a long cleanup doesn't hold a large transaction.
            await _session.SaveChangesAsync(cancellationToken);
            deleted += old.Count;
            context.ReportProgress(null, $"{deleted} old runs deleted.");
        }

        context.LogInformation($"{deleted} runs older than {_options.RunRetentionDays} days were deleted.");
    }
}
