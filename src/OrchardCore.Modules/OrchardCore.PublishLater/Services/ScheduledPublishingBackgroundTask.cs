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
using YesSql;

namespace OrchardCore.PublishLater.Services
{
    [BackgroundTask(Schedule = "* * * * *", Description = "Publishes content items when their scheduled publish date time arrives.")]
    public class ScheduledPublishingBackgroundTask : IBackgroundTask
    {
        private readonly ILogger<ScheduledPublishingBackgroundTask> _logger;
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
                .Query<ContentItem, PublishLaterPartIndex>(index => index.ScheduledPublishUtc < _clock.UtcNow)
                .ListAsync();

            if (!itemsToPublish.Any())
            {
                return;
            }

            foreach (var item in itemsToPublish)
            {
                _logger.LogDebug("Publishing scheduled content item {ContentItemId}.", item.ContentItemId);
                await serviceProvider.GetRequiredService<IContentManager>().PublishAsync(item);
            }
        }
    }
}
