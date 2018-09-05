using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.WebHooks.Abstractions.Services;
using OrchardCore.WebHooks.Models;

namespace OrchardCore.WebHooks.Handlers
{
    public class ContentsHandler : ContentHandlerBase
    {
        private readonly IWebHookManager _webhooksManager;

        public ContentsHandler(IWebHookManager webhooksManager)
        {
            _webhooksManager = webhooksManager;
        }

        public override Task CreatedAsync(CreateContentContext context)
        {
            return TriggerContentEvent(ContentEvents.Created, context);
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            return TriggerContentEvent(ContentEvents.Updated, context);
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            return TriggerContentEvent(ContentEvents.Published, context);
        }

        public override Task UnpublishedAsync(PublishContentContext context)
        {
            return TriggerContentEvent(ContentEvents.Unpublished, context);
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            return TriggerContentEvent(ContentEvents.Removed, context);
        }

        private Task TriggerContentEvent(string eventName, ContentContextBase context)
        {
            var contentItem = context.ContentItem;
            
            // Setup the properties used in the liquid templates
            var properties = new Dictionary<string, object>
            {
                {"Model", contentItem}
            };

            return Task.WhenAll(
                // Trigger webhooks for the general content.{event} event
                _webhooksManager.NotifyAsync($"content.{eventName}", contentItem.Content, properties),

                // Trigger webhooks for the more specific {content type}.{event} event e.g. article.created
                _webhooksManager.NotifyAsync($"{contentItem.ContentType.ToLower()}.{eventName}", contentItem.Content, properties)
            );
        }
    }
}