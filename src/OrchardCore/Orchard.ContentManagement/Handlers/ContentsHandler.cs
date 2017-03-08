using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;

namespace Orchard.ContentManagement.Handlers
{
    public class UpdateContentsHandler : ContentHandlerBase
    {
        private readonly IClock _clock;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UpdateContentsHandler(IClock clock, IHttpContextAccessor httpContextAccessor)
        {
            _clock = clock;
            _httpContextAccessor = httpContextAccessor;
        }

        public override void Creating(CreateContentContext context)
        {
            var utcNow = _clock.UtcNow;
            context.ContentItem.CreatedUtc = utcNow;
            context.ContentItem.ModifiedUtc = utcNow;

            var httpContext = _httpContextAccessor.HttpContext;
            if (context.ContentItem.Owner == null && (httpContext?.User?.Identity?.IsAuthenticated ?? false))
            {
                context.ContentItem.Owner = httpContext.User.Identity.Name;
            }
        }

        public override void Updating(UpdateContentContext context)
        {
            var utcNow = _clock.UtcNow;
            context.ContentItem.ModifiedUtc = utcNow;
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated ?? false)
            {
                // The value is only modified during update so that another event like
                // publishing in a Workflow doesn't change it.
                context.ContentItem.Author = httpContext.User.Identity.Name;
            }
        }

        public override void Versioning(VersionContentContext context)
        {
            var utcNow = _clock.UtcNow;

            context.BuildingContentItem.CreatedUtc = context.ContentItem.CreatedUtc ?? utcNow;
            context.BuildingContentItem.PublishedUtc = context.ContentItem.PublishedUtc;
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
