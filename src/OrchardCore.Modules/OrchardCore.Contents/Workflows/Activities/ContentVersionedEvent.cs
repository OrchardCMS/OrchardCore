using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentVersionedEvent : ContentEvent
    {
        public ContentVersionedEvent(IContentManager contentManager, IStringLocalizer<ContentCreatedEvent> s) : base(contentManager, s)
        {
        }

        public override string Name => nameof(ContentVersionedEvent);
        public override LocalizedString Description => S["Content is versioned."];
    }
}