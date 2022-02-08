using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.ContentManagement;
using OrchardCore.PublishLater.Models;

namespace OrchardCore.PublishLater.Services
{
    public class PublishLaterBackgroundJobHandler : BackgroundJobHandler<PublishLaterBackgroundJob>
    {
        private readonly IContentManager _contentManager;
        private readonly ILogger _logger;

        public PublishLaterBackgroundJobHandler(IContentManager contentManager, ILogger<PublishLaterBackgroundJobHandler> logger)
        {
            _contentManager = contentManager;
            _logger = logger;
        }

        public override async ValueTask ExecuteAsync(PublishLaterBackgroundJob backgroundJob, CancellationToken cancellationToken)
        {
            var contentItem = await _contentManager.GetAsync(backgroundJob.CorrelationId, VersionOptions.Latest);
            if (contentItem is not null)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Publishing scheduled content item {ContentItemId}.", contentItem.ContentItemId);
                }
                await _contentManager.PublishAsync(contentItem);
            }
        }
    }
}
