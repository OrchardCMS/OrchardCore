using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Modules;
using OrchardCore.PublishLater.Indexes;
using YesSql;

namespace OrchardCore.PublishLater.Services;

/// <summary>
/// Finds the content items whose scheduled publish date arrived, and queues a run of <see cref="PublishLaterJob"/>
/// for each of them. Every server looks for them, which only reads the database; the runs are what publishes, and
/// a run is executed by a single server.
/// </summary>
[BackgroundTask(
    Title = "Scheduled Content Items Publisher",
    Schedule = "* * * * *",
    Description = "Queues the publication of the content items whose scheduled publish date time arrived.")]
public sealed class ScheduledPublishingBackgroundTask : IBackgroundTask
{
    // After a publication failed, the content item is tried again after this delay, not every minute.
    private static readonly TimeSpan _retryAfter = TimeSpan.FromHours(1);

    private readonly IClock _clock;

    public ScheduledPublishingBackgroundTask(IClock clock)
    {
        _clock = clock;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        var itemsToPublish = await serviceProvider
            .GetRequiredService<ISession>()
            .QueryIndex<PublishLaterPartIndex>(index => index.Latest && !index.Published && index.ScheduledPublishDateTimeUtc < now)
            .ListAsync(cancellationToken);

        if (itemsToPublish.Count == 0)
        {
            return;
        }

        var jobManager = serviceProvider.GetRequiredService<IJobManager>();
        var contentManager = serviceProvider.GetRequiredService<IContentManager>();

        foreach (var item in itemsToPublish)
        {
            var last = await jobManager.FindLastAsync(PublishLaterJob.Name, item.ContentItemId);

            if (last is not null && (!last.IsCompleted || (last.Status == JobRunStatus.Failed && last.CompletedUtc > now - _retryAfter)))
            {
                continue;
            }

            var contentItem = await contentManager.GetAsync(item.ContentItemId, VersionOptions.Latest);

            // The same key makes the servers that find the content item at the same time queue a single run, and
            // makes two runs for the same content item execute one after the other.
            await jobManager.EnqueueAsync<PublishLaterJob>(new PublishLaterJobParameters { ContentItemId = item.ContentItemId }, request =>
            {
                request.Title = contentItem?.DisplayText ?? item.ContentItemId;
                request.Trigger = JobTriggers.Schedule;
                request.DeduplicationKey = item.ContentItemId;
                request.ConcurrencyKey = $"{PublishLaterJob.Name}:{item.ContentItemId}";
                request.CorrelationId = item.ContentItemId;
            });
        }
    }
}
