using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentPublishedEvent : ContentEvent
    {
        public ContentPublishedEvent(IContentManager contentManager, IStringLocalizer<ContentCreatedEvent> s) : base(contentManager, s)
        {
        }

        public override string Name => nameof(ContentPublishedEvent);
        public override LocalizedString Description => S["Content is published."];
    }
}