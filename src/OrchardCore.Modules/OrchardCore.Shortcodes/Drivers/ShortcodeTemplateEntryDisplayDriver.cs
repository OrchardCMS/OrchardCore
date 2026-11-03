using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Shortcodes.ViewModels;

namespace OrchardCore.Shortcodes.Drivers;

/// <summary>
/// Builds the row of the shortcode templates admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <c>Shortcodes</c> renders.
/// </summary>
public sealed class ShortcodeTemplateEntryDisplayDriver : DisplayDriver<ShortcodeTemplateEntry>
{
    public override Task<IDisplayResult> DisplayAsync(ShortcodeTemplateEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ShortcodeTemplateEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"),
            View("ShortcodeTemplateEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("ShortcodeTemplateEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:5"),
            View("ShortcodeTemplateEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Categories:5"),
            View("ShortcodeTemplateEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
        );
    }
}
