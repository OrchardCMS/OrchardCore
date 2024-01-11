using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.PublishLater.Indexes;
using OrchardCore.PublishLater.Models;
using YesSql;

namespace OrchardCore.PublishLater.Services;

[BackgroundTask(
    Title = "Scheduled Content Items Publisher",
    Schedule = "* * * * *",
    Description = "Publishes content items when their scheduled publish date time arrives.")]
public class ScheduledPublishingBackgroundTask : IBackgroundTask
{
    private readonly ILogger _logger;
    private readonly IClock _clock;

    public ScheduledPublishingBackgroundTask(ILogger<ScheduledPublishingBackgroundTask> logger, IClock clock)
    {
        _logger = logger;
        _clock = clock;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var itemsToPublish = await serviceProvider
            .GetRequiredService<ISession>()
            .QueryIndex<PublishLaterPartIndex>(index => index.Latest && !index.Published && index.ScheduledPublishDateTimeUtc < _clock.UtcNow)
            .ListAsync();

        if (!itemsToPublish.Any())
        {
            return;
        }

        var contentManager = serviceProvider.GetRequiredService<IContentManager>();

        foreach (var item in itemsToPublish)
        {
            var contentItem = await contentManager.GetAsync(item.ContentItemId, VersionOptions.Latest);

            var part = contentItem.As<PublishLaterPart>();
            if (part != null)
            {
                part.ScheduledPublishUtc = null;
                part.Apply();
            }

            _logger.LogDebug("Publishing scheduled content item {ContentItemId}.", contentItem.ContentItemId);

            await contentManager.PublishAsync(contentItem);
        }
    }
}
