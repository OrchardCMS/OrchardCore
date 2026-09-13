using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.BackgroundTasks.Drivers;

/// <summary>
/// Builds the row of the background tasks admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="BackgroundTasksAdminList"/> renders.
/// </summary>
public sealed class BackgroundTaskEntryDisplayDriver : DisplayDriver<BackgroundTaskEntry>
{
    public override Task<IDisplayResult> DisplayAsync(BackgroundTaskEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("BackgroundTaskEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("BackgroundTaskEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("BackgroundTaskEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5"),
            View("BackgroundTaskEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("BackgroundTaskEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
