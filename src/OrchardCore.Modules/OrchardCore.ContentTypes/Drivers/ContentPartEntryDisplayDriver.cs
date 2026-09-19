using OrchardCore.ContentTypes.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Drivers;

/// <summary>
/// Builds the row of the content parts admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="ContentPartsAdminList"/> renders.
/// </summary>
public sealed class ContentPartEntryDisplayDriver : DisplayDriver<ContentPartEntry>
{
    public override Task<IDisplayResult> DisplayAsync(ContentPartEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ContentPartEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("ContentPartEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5"),
            View("ContentPartEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
