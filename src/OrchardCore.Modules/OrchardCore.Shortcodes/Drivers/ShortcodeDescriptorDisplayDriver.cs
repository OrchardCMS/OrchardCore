using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Shortcodes.Drivers;

public sealed class ShortcodeDescriptorDisplayDriver : DisplayDriver<ShortcodeDescriptor>
{
    public override Task<IDisplayResult> DisplayAsync(ShortcodeDescriptor descriptor, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ShortcodeDescriptor_Fields_SummaryAdmin", descriptor).Location("SummaryAdmin", "Content"),
            View("ShortcodeDescriptor_SummaryAdmin__Button__Actions", descriptor).Location("SummaryAdmin", "Actions")
        );
    }
}
