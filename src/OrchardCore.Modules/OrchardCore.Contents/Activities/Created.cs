using Microsoft.Extensions.Localization;

namespace OrchardCore.Contents.Activities
{
    public class Created : ContentEvent
    {
        public Created(IStringLocalizer<Created> s) : base(s)
        {
        }

        public override string Name => nameof(Created);
        public override LocalizedString Description => S["Content is created."];
    }
}