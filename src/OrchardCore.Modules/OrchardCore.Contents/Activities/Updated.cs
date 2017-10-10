using Microsoft.Extensions.Localization;

namespace OrchardCore.Contents.Activities
{
    public class Updated : ContentEvent
    {
        public Updated(IStringLocalizer<Updated> s) : base(s)
        {
        }

        public override string Name => nameof(Updated);
        public override LocalizedString Description => S["Content is updated."];
    }
}