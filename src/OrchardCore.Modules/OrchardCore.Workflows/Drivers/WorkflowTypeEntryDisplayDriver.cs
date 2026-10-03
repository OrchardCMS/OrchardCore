using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

/// <summary>
/// Builds the row of the workflow types admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="WorkflowTypesAdminList"/> renders.
/// </summary>
public sealed class WorkflowTypeEntryDisplayDriver : DisplayDriver<WorkflowTypeEntry>
{
    public override Task<IDisplayResult> DisplayAsync(WorkflowTypeEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("WorkflowTypeEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("WorkflowTypeEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("WorkflowTypeEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("WorkflowTypeEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5"),
            View("WorkflowTypeEntry_ActionsMenuItems_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
        );
    }
}
