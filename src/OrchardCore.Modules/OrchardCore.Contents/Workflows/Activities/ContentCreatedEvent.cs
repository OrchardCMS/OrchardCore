using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentCreatedEvent : ContentEvent
    {
        public ContentCreatedEvent(IContentManager contentManager, IStringLocalizer<ContentCreatedEvent> s) : base(contentManager, s)
        {
        }

        public override string Name => nameof(ContentCreatedEvent);
        public override LocalizedString Description => S["Content is created."];
    }
}