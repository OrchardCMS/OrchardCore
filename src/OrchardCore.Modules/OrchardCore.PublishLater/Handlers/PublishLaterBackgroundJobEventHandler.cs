using System;
using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Events;
using OrchardCore.ContentManagement;
using OrchardCore.PublishLater.Models;

namespace OrchardCore.PublishLater.Handlers
{
    public class PublishLaterBackgroundJobEventHandler : BackgroundJobEventBase
    {
        private readonly IContentManager _contentManager;

        public PublishLaterBackgroundJobEventHandler(IContentManager contentManager) => _contentManager = contentManager;

        public async override ValueTask UpdatedAsync(BackgroundJobUpdateContext context)
        {
            if (context.BackgroundJobExecution.State.CurrentStatus != BackgroundJobs.Models.BackgroundJobStatus.Cancelled)
            {
                return;
            }

            if (!context.BackgroundJobExecution.BackgroundJob.Name.Equals(nameof(PublishLaterBackgroundJob), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }


            var contentItem = await _contentManager.GetAsync(context.BackgroundJobExecution.BackgroundJob.CorrelationId, VersionOptions.Latest);
            if (contentItem is null)
            {
                return;
            }

            contentItem.Alter<PublishLaterPart>(x => x.ScheduledPublishUtc = null);

            await _contentManager.UpdateAsync(contentItem);
        }
    }
}
