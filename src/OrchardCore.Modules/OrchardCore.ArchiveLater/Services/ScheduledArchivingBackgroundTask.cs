using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.ContentManagement;
using OrchardCore.Modules;
using OrchardCore.ArchiveLater.Indexes;
using YesSql;

namespace OrchardCore.ArchiveLater.Services
{
    [BackgroundTask(Schedule = "* * * * *", Description = "Archives content items when their scheduled archive date time arrives.")]
    public class ScheduledArchivingBackgroundTask : IBackgroundTask
    {
        private readonly ILogger<ScheduledArchivingBackgroundTask> _logger;
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
                .Query<ContentItem, ArchiveLaterPartIndex>(index => index.ScheduledArchiveDateTimeUtc < _clock.UtcNow)
                .ListAsync();

            if (!itemsToArchive.Any())
            {
                return;
            }

            foreach (var item in itemsToArchive)
            {
                _logger.LogDebug("Archiving scheduled content item {ContentItemId}.", item.ContentItemId);

                await serviceProvider.GetRequiredService<IContentManager>().UnpublishAsync(item);
            }
        }
    }
}
