using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentDeletedEvent : ContentEvent
    {
        public ContentDeletedEvent(IContentManager contentManager, IStringLocalizer<ContentCreatedEvent> s) : base(contentManager, s)
        {
        }

        public override string Name => nameof(ContentDeletedEvent);
        public override LocalizedString Description => S["Content is deleted."];
    }
}