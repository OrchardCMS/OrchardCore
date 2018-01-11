using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentDeletedEvent : ContentEvent
    {
        public ContentDeletedEvent(IContentManager contentManager, IStringLocalizer<ContentCreatedEvent> localizer) : base(contentManager, localizer)
        {
        }

        public override string Name => nameof(ContentDeletedEvent);
        public override LocalizedString Description => T["Content is deleted."];
    }
}