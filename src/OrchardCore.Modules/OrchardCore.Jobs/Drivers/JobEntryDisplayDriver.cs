using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Jobs.ViewModels;

namespace OrchardCore.Jobs.Drivers;

/// <summary>
/// Builds the row of the jobs admin list. Each shape is placed in a zone of the row for the List layout, and in
/// the zone of a column of <c>Jobs</c> for the Grid layout.
/// </summary>
public sealed class JobEntryDisplayDriver : DisplayDriver<JobEntry>
{
    public override Task<IDisplayResult> DisplayAsync(JobEntry entry, BuildDisplayContext context)
    {
        var results = new List<IDisplayResult>
        {
            View("JobEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Title:1"),
            View("JobEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Title:5"),
            View("JobEntry_Schedule_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Schedule:5"),
            View("JobEntry_LastRun_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "LastRun:5"),
            View("JobEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
        };

        if (entry.CanManage)
        {
            results.Add(View("JobEntry_ActionsMenuItems_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:5"));
        }

        return CombineAsync(results);
    }
}
