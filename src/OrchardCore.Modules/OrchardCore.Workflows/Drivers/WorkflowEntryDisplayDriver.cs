using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers;

/// <summary>
/// Builds the row of the workflow instances admin list. Each shape is placed in the zone that the matching
/// <see cref="Admin.Models.AdminListColumn"/> of <see cref="WorkflowInstancesAdminList"/> renders.
/// </summary>
public sealed class WorkflowEntryDisplayDriver : DisplayDriver<WorkflowEntry>
{
    public override Task<IDisplayResult> DisplayAsync(WorkflowEntry entry, BuildDisplayContext context)
    {
        return CombineAsync(
            View("WorkflowEntry_Checkbox_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Checkbox:1"),
            View("WorkflowEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1"),
            View("WorkflowEntry_DefaultTags_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5"),
            View("WorkflowEntry_DefaultMeta_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5"),
            View("WorkflowEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
        );
    }
}
