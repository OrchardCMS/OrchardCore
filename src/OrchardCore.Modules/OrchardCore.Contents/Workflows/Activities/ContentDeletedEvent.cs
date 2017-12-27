using Microsoft.Extensions.Localization;

namespace OrchardCore.Contents.Workflows.Activities
{
    public class ContentDeletedEvent : ContentEvent
    {
        public ContentDeletedEvent(IStringLocalizer<ContentDeletedEvent> s) : base(s)
        {
        }

        public override string Name => nameof(ContentDeletedEvent);
        public override LocalizedString Description => S["Content is deleted."];
    }
}