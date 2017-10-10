using Microsoft.Extensions.Localization;

namespace OrchardCore.Contents.Activities
{
    public class Deleted : ContentEvent
    {
        public Deleted(IStringLocalizer<Deleted> s) : base(s)
        {
        }

        public override string Name => nameof(Deleted);
        public override LocalizedString Description => S["Content is deleted."];
    }
}