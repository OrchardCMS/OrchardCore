using Microsoft.Extensions.Localization;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentCreatedEvent : ContentEvent
    {
        public ContentCreatedEvent(IStringLocalizer<ContentCreatedEvent> s) : base(s)
        {
        }

        public override string Name => nameof(ContentCreatedEvent);
        public override LocalizedString Description => S["Content is created."];
    }
}