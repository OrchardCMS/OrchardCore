using Microsoft.Extensions.Localization;

namespace OrchardCore.Contents.Activities
{
    public class Versioned : ContentEvent
    {
        public Versioned(IStringLocalizer<Versioned> s) : base(s)
        {
        }

        public override string Name => nameof(Versioned);
        public override LocalizedString Description => S["Content is versioned."];
    }
}