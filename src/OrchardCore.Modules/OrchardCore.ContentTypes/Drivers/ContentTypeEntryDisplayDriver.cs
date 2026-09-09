using OrchardCore.ContentTypes.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentTypes.Drivers;

/// <summary>
/// Builds the row of the content types admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="ContentTypesAdminList"/> renders.
/// </summary>
public sealed class ContentTypeEntryDisplayDriver : DisplayDriver<ContentTypeEntry>
{
    public override Task<IDisplayResult> DisplayAsync(ContentTypeEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("ContentTypeEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("ContentTypeEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5"),
            View("ContentTypeEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("ContentTypeEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
