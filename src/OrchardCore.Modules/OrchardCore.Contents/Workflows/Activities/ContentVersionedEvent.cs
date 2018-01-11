using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentVersionedEvent : ContentEvent
    {
        public ContentVersionedEvent(IContentManager contentManager, IStringLocalizer<ContentCreatedEvent> localizer) : base(contentManager, localizer)
        {
        }

        public override string Name => nameof(ContentVersionedEvent);
        public override LocalizedString Description => T["Content is versioned."];
    }
}