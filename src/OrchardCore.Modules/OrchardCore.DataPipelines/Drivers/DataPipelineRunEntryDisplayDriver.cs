using OrchardCore.DataPipelines.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DataPipelines.Drivers;

/// <summary>
/// Builds the row of the run history, in both layouts of the list: each shape is placed in a zone of the List row
/// (<c>SummaryAdmin</c>) and in the column of the Grid (<c>SummaryAdminGrid</c>).
/// </summary>
internal sealed class DataPipelineRunEntryDisplayDriver : DisplayDriver<DataPipelineRunEntry>
{
    public override Task<IDisplayResult> DisplayAsync(DataPipelineRunEntry entry, BuildDisplayContext context)
    {
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape row)
        {
            row.Attributes["data-run-id"] = entry.Run.RunId;
        }

        return CombineAsync(
            View("DataPipelineRunEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Pipeline:1"),
            View("DataPipelineRunEntry_Error_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Pipeline:5")
                .RenderWhen(() => Task.FromResult(!string.IsNullOrEmpty(entry.Run.Error))),
            View("DataPipelineRunEntry_Status_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Status:5"),
            View("DataPipelineRunEntry_Version_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Version:5"),
            View("DataPipelineRunEntry_Queued_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Queued:5"),
            View("DataPipelineRunEntry_Duration_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Duration:5"),
            View("DataPipelineRunEntry_Trigger_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:15")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "StartedBy:5"),
            View("DataPipelineRunEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
            View("DataPipelineRunEntry_ActionsMenuItems_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:5"));
    }
}
