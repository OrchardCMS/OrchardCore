using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.ArchiveLater.Indexes;
using OrchardCore.ArchiveLater.Models;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.Entities;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.ArchiveLater.Services;

[BackgroundTask(
    Title = "Content Items Archiver",
    Schedule = "* * * * *",
    Description = "Archives content items when their scheduled archive date time arrives.")]
public class ScheduledArchivingBackgroundTask : IBackgroundTask
{
    private readonly ILogger _logger;
    private readonly IClock _clock;

    public ScheduledArchivingBackgroundTask(ILogger<ScheduledArchivingBackgroundTask> logger, IClock clock)
    {
        _logger = logger;
        _clock = clock;
    }

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var itemsToArchive = await serviceProvider
            .GetRequiredService<ISession>()
            .QueryIndex<ArchiveLaterPartIndex>(index => index.Latest && index.Published && index.ScheduledArchiveDateTimeUtc < _clock.UtcNow)
            .ListAsync();

        if (!itemsToArchive.Any())
        {
            return;
        }

        var contentManager = serviceProvider.GetRequiredService<IContentManager>();

        foreach (var item in itemsToArchive)
        {
            var contentItem = await contentManager.GetAsync(item.ContentItemId);

            var part = contentItem.As<ArchiveLaterPart>();
            if (part != null)
            {
                part.ScheduledArchiveUtc = null;
                part.Apply();
            }

            _logger.LogDebug("Archiving scheduled content item {ContentItemId}.", contentItem.ContentItemId);

            await contentManager.UnpublishAsync(contentItem);
        }
    }
}
