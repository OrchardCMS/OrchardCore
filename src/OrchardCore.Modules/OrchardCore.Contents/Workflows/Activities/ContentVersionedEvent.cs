using Microsoft.Extensions.Localization;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentVersionedEvent : ContentEvent
    {
        public ContentVersionedEvent(IStringLocalizer<ContentVersionedEvent> s) : base(s)
        {
        }

        public override string Name => nameof(ContentVersionedEvent);
        public override LocalizedString Description => S["Content is versioned."];
    }
}