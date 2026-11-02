using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.ContentTypes.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Drivers;

/// <summary>
/// Builds the row of the content parts admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <c>ContentParts</c> renders.
/// </summary>
public sealed class ContentPartEntryDisplayDriver : DisplayDriver<ContentPartEntry>
{
    public override Task<IDisplayResult> DisplayAsync(ContentPartEntry entry, BuildDisplayContext context)
    {
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape rowShape)
        {
            rowShape.Attributes["data-filter-value"] = entry.DisplayName;
            rowShape.Attributes["data-type-name"] = entry.Name;
        }

        return CombineAsync(
            View("ContentPartEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "DisplayName:1"),
            View("ContentPartEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "DisplayName:5"),
            View("ContentPartEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5")
        );
    }
}
