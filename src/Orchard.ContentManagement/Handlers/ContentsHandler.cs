using Orchard.Services;

namespace Orchard.ContentManagement.Handlers
{
    public class UpdateContentsHandler : ContentHandlerBase
    {
        private readonly IClock _clock;

        public UpdateContentsHandler(IClock clock)
        {
            _clock = clock;
        }

        public override void Initializing(InitializingContentContext context)
        {
            var utcNow = _clock.UtcNow;
            context.ContentItem.CreatedUtc = utcNow;
            context.ContentItem.ModifiedUtc = utcNow;
        }

        public override void Updating(UpdateContentContext context)
        {
            var utcNow = _clock.UtcNow;
            context.ContentItem.ModifiedUtc = utcNow;
        }

        public override void Versioning(VersionContentContext context)
        {
            var utcNow = _clock.UtcNow;

            context.BuildingContentItem.CreatedUtc = context.ExistingContentItem.CreatedUtc ?? utcNow;
            context.BuildingContentItem.PublishedUtc = context.ExistingContentItem.PublishedUtc;
            context.BuildingContentItem.ModifiedUtc = utcNow;
        }

        public override void Published(PublishContentContext context)
        {
            var utcNow = _clock.UtcNow;

            // The first time the content is published, reassign the CreateUtc value
            if(!context.ContentItem.PublishedUtc.HasValue)
            {
                context.ContentItem.CreatedUtc = utcNow;
            }

            context.ContentItem.PublishedUtc = utcNow;
        }

        public override void Unpublished(PublishContentContext context)
        {
            var utcNow = _clock.UtcNow;
            context.ContentItem.PublishedUtc = null;
        }
    }
}
