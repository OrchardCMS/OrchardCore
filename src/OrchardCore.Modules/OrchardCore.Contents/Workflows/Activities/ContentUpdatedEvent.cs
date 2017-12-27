using Microsoft.Extensions.Localization;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentUpdatedEvent : ContentEvent
    {
        public ContentUpdatedEvent(IStringLocalizer<ContentUpdatedEvent> s) : base(s)
        {
        }

        public override string Name => nameof(ContentUpdatedEvent);
        public override LocalizedString Description => S["Content is updated."];
    }
}