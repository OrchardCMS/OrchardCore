using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.DataPipelines.Jobs;

/// <summary>
/// Ends the data pipeline run whose job run ended without executing it to its end: it was cancelled before it
/// started, or its server stopped.
/// </summary>
public sealed class DataPipelineJobRunHandler : IJobRunHandler
{
    private readonly ISession _session;
    private readonly IClock _clock;

    public DataPipelineJobRunHandler(ISession session, IClock clock)
    {
        _session = session;
        _clock = clock;
    }

    public async Task CompletedAsync(JobRun jobRun)
    {
        ArgumentNullException.ThrowIfNull(jobRun);

        if (jobRun.JobName != DataPipelineRunJob.Name || string.IsNullOrEmpty(jobRun.CorrelationId))
        {
            return;
        }

        var runId = jobRun.CorrelationId;
        var run = await _session.Query<DataPipelineRun, DataPipelineRunIndex>(index => index.RunId == runId).FirstOrDefaultAsync();

        // The run that executed to its end recorded how it ended.
        if (run is null || run.IsCompleted)
        {
            return;
        }

        var cancelled = jobRun.Status == JobRunStatus.Canceled;

        run.Status = cancelled ? DataPipelineRunStatus.Cancelled : DataPipelineRunStatus.Failed;
        run.CompletedUtc = _clock.UtcNow;
        run.Error = cancelled
            ? (run.StartedUtc is null ? "The run was cancelled before it started." : "The run was cancelled.")
            : jobRun.Error ?? "The run stopped unexpectedly, such as when the site restarted.";

        await _session.SaveAsync(run);

        DataPipelineRunNotifications.NotifyAfterCommit(run);
    }
}
