using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentUpdatedEvent : ContentEvent
    {
        public ContentUpdatedEvent(IContentManager contentManager, IStringLocalizer<ContentCreatedEvent> s) : base(contentManager, s)
        {
        }

        public override string Name => nameof(ContentUpdatedEvent);
        public override LocalizedString Description => S["Content is updated."];
    }
}