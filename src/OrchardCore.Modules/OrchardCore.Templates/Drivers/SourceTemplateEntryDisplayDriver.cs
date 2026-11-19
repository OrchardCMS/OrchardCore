using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Drivers;

/// <summary>
/// Builds the row of a template source admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <c>TemplateSources</c> renders.
/// </summary>
public sealed class SourceTemplateEntryDisplayDriver : DisplayDriver<SourceTemplateEntry>
{
    public override Task<IDisplayResult> DisplayAsync(SourceTemplateEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("SourceTemplateEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"),
            View("SourceTemplateEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("SourceTemplateEntry_Status_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Status:5"),
            View("SourceTemplateEntry_Path_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Source:5"),
            View("SourceTemplateEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
        );
    }
}
