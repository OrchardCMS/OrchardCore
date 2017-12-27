using Microsoft.Extensions.Localization;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentPublishedEvent : ContentEvent
    {
        public ContentPublishedEvent(IStringLocalizer<ContentPublishedEvent> s) : base(s)
        {
        }

        public override string Name => nameof(ContentPublishedEvent);
        public override LocalizedString Description => S["Content is published."];
    }
}