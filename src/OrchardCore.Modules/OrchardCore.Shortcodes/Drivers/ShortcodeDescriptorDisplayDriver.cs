using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Shortcodes.Drivers
{
    public class ShortcodeDescriptorDisplayDriver : DisplayDriver<ShortcodeDescriptor>
    {
        public override IDisplayResult Display(ShortcodeDescriptor descriptor)
        {
            return Combine(
                View("ShortcodeDescriptor_SummaryAdmin__Content", descriptor).Location("SummaryAdmin", "Content"),
                View("ShortcodeDescriptor_SummaryAdmin__Button__Actions", descriptor).Location("SummaryAdmin", "Actions")
            );
        }
    }
}
