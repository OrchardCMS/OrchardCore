using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentPublishedEvent : ContentEvent
    {
        public ContentPublishedEvent(IContentManager contentManager, IStringLocalizer<ContentCreatedEvent> localizer) : base(contentManager, localizer)
        {
        }

        public override string Name => nameof(ContentPublishedEvent);
        public override LocalizedString Description => T["Content is published."];
    }
}