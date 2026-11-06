using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.Drivers;

/// <summary>
/// Builds the row of the alternates list of a template. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <c>TemplateAlternates</c> renders.
/// </summary>
public sealed class TemplateAlternateEntryDisplayDriver : DisplayDriver<TemplateAlternateEntry>
{
    public override Task<IDisplayResult> DisplayAsync(TemplateAlternateEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("TemplateAlternateEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("TemplateAlternateEntry_Status_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Status:5"),
            View("TemplateAlternateEntry_AppliesTo_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "AppliesTo:5"),
            View("TemplateAlternateEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
        );
    }
}
