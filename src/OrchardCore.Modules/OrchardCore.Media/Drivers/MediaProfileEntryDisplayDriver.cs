using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Drivers;

/// <summary>
/// Builds the row of the media profiles admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="MediaProfilesAdminList"/> renders.
/// </summary>
public sealed class MediaProfileEntryDisplayDriver : DisplayDriver<MediaProfileEntry>
{
    public override Task<IDisplayResult> DisplayAsync(MediaProfileEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("MediaProfileEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("MediaProfileEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("MediaProfileEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5"),
            View("MediaProfileEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
