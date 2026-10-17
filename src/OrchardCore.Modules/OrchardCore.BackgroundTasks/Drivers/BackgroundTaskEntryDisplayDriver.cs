using System.Globalization;
using OrchardCore.BackgroundTasks.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.BackgroundTasks.Drivers;

/// <summary>
/// Builds the row of the background tasks admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <c>BackgroundTasks</c> renders.
/// </summary>
public sealed class BackgroundTaskEntryDisplayDriver : DisplayDriver<BackgroundTaskEntry>
{
    public override Task<IDisplayResult> DisplayAsync(BackgroundTaskEntry entry, BuildDisplayContext context)
    {
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape rowShape)
        {
            // Rendered on the row element in every layout, so that the page can follow the execution state of the task.
            rowShape.Attributes["data-task-name"] = entry.Name;
            rowShape.Attributes["data-task-status"] = entry.State?.Status.ToString() ?? string.Empty;
            rowShape.Attributes["data-task-run-count"] = entry.State?.RunCount.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        }

        var results = new List<IDisplayResult>
        {
            View("BackgroundTaskEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Select:1"),
            View("BackgroundTaskEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Title:1"),
            View("BackgroundTaskEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Title:5"),
            View("BackgroundTaskEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Status:5"),
            View("BackgroundTaskEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
            View("BackgroundTaskEntry_ActionsMenuItems_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:5"),
        };

        if (entry.IsMonitored)
        {
            // The execution state: badges beside the title in the list, columns of their own in the grid.
            results.Add(View("BackgroundTaskEntry_Status_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Status:10"));
            results.Add(View("BackgroundTaskEntry_LastRun_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:20")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "LastRun:5"));
            results.Add(View("BackgroundTaskEntry_NextRun_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:30")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "NextRun:5"));
            results.Add(View("BackgroundTaskEntry_Error_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Title:10"));
        }

        return CombineAsync(results);
    }
}
