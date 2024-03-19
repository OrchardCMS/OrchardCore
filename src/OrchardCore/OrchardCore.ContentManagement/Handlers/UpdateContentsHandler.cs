using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Modules;

namespace OrchardCore.ContentManagement.Handlers
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

        public override Task CreatingAsync(CreateContentContext context)
        {
            var utcNow = _clock.UtcNow;
            if (!context.ContentItem.CreatedUtc.HasValue)
            {
                context.ContentItem.CreatedUtc = utcNow;
            }

            context.ContentItem.ModifiedUtc = utcNow;

            var httpContext = _httpContextAccessor.HttpContext;
            if (context.ContentItem.Owner == null && (httpContext?.User?.Identity?.IsAuthenticated ?? false))
            {
                context.ContentItem.Owner = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                context.ContentItem.Author = httpContext.User.Identity.Name;
            }

            return Task.CompletedTask;
        }

        public override Task UpdatingAsync(UpdateContentContext context)
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

            return Task.CompletedTask;
        }

        public override Task VersioningAsync(VersionContentContext context)
        {
            var utcNow = _clock.UtcNow;

            context.BuildingContentItem.Owner = context.ContentItem.Owner;
            context.BuildingContentItem.CreatedUtc = context.ContentItem.CreatedUtc ?? utcNow;
            context.BuildingContentItem.PublishedUtc = context.ContentItem.PublishedUtc;
            context.BuildingContentItem.ModifiedUtc = utcNow;

            return Task.CompletedTask;
        }

        public override Task PublishingAsync(PublishContentContext context)
        {
            var utcNow = _clock.UtcNow;

            // The first time the content is published, reassign the CreateUtc value if it has not already been set.
            if (!context.ContentItem.PublishedUtc.HasValue && !context.ContentItem.CreatedUtc.HasValue)
            {
                context.ContentItem.CreatedUtc = utcNow;
            }

            context.ContentItem.PublishedUtc = utcNow;

            return Task.CompletedTask;
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            context.ContentItem.PublishedUtc = null;
            return Task.CompletedTask;
        }
    }
}
