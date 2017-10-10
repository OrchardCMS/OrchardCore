using Microsoft.Extensions.Localization;

namespace OrchardCore.Contents.Activities
{
    public class Published : ContentEvent
    {
        public Published(IStringLocalizer<Published> s) : base(s)
        {
        }

        public override string Name => nameof(Published);
        public override LocalizedString Description => S["Content is published."];
    }
}