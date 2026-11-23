using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ArchiveLater.Indexes;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.ArchiveLater.Services;

/// <summary>
/// Finds the content items whose scheduled archive date arrived, and queues a run of <see cref="ArchiveLaterJob"/>
/// for each of them. Every server looks for them, which only reads the database; the runs are what archives, and a
/// run is executed by a single server.
/// </summary>
[BackgroundTask(
    Title = "Content Items Archiver",
    Schedule = "* * * * *",
    Description = "Queues the archiving of the content items whose scheduled archive date time arrived.")]
public sealed class ScheduledArchivingBackgroundTask : IBackgroundTask
{
    // After an archiving failed, the content item is tried again after this delay, not every minute.
    private static readonly TimeSpan _retryAfter = TimeSpan.FromHours(1);

    private readonly IClock _clock;

    public ScheduledArchivingBackgroundTask(IClock clock)
    {
        _clock = clock;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        var itemsToArchive = await serviceProvider
            .GetRequiredService<ISession>()
            .QueryIndex<ArchiveLaterPartIndex>(index => index.Latest && index.Published && index.ScheduledArchiveDateTimeUtc < now)
            .ListAsync(cancellationToken);

        if (itemsToArchive.Count == 0)
        {
            return;
        }

        var jobManager = serviceProvider.GetRequiredService<IJobManager>();
        var contentManager = serviceProvider.GetRequiredService<IContentManager>();

        foreach (var item in itemsToArchive)
        {
            var last = await jobManager.FindLastAsync(ArchiveLaterJob.Name, item.ContentItemId);

            if (last is not null && (!last.IsCompleted || (last.Status == JobRunStatus.Failed && last.CompletedUtc > now - _retryAfter)))
            {
                continue;
            }

            var contentItem = await contentManager.GetAsync(item.ContentItemId);

            // The same key makes the servers that find the content item at the same time queue a single run, and
            // makes two runs for the same content item execute one after the other.
            await jobManager.EnqueueAsync<ArchiveLaterJob>(new ArchiveLaterJobParameters { ContentItemId = item.ContentItemId }, request =>
            {
                request.Title = contentItem?.DisplayText ?? item.ContentItemId;
                request.Trigger = JobTriggers.Schedule;
                request.DeduplicationKey = item.ContentItemId;
                request.ConcurrencyKey = $"{ArchiveLaterJob.Name}:{item.ContentItemId}";
                request.CorrelationId = item.ContentItemId;
            });
        }
    }
}
