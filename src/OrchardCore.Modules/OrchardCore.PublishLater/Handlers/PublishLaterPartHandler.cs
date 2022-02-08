using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundJobs;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.PublishLater.Models;

namespace OrchardCore.PublishLater.Handlers
{
    public class PublishLaterPartHandler : ContentPartHandler<PublishLaterPart>
    {
        private readonly IServiceProvider _serviceProvider;

        private IBackgroundJobService _backgroundJobService;

        public PublishLaterPartHandler(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public override async Task PublishedAsync(PublishContentContext context, PublishLaterPart part)
        {
            part.ScheduledPublishUtc = null;
            part.Apply();

            _backgroundJobService ??= _serviceProvider.GetRequiredService<IBackgroundJobService>();

            var job = await _backgroundJobService.GetByCorrelationIdAsync<PublishLaterBackgroundJob>(part.ContentItem.ContentItemId);
            if (job.BackgroundJob is not null && (job.State?.CurrentStatus != BackgroundJobStatus.Scheduled || job.State?.CurrentStatus != BackgroundJobStatus.Queued))
            {
                await _backgroundJobService.TryCancelAsync(job.BackgroundJob.BackgroundJobId);
            }
        }
    }
}
