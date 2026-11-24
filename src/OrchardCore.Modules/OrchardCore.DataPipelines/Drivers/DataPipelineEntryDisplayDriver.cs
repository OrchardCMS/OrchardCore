using OrchardCore.DataPipelines.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DataPipelines.Drivers;

/// <summary>
/// Builds the row of the list of the pipelines, in both layouts of the list: each shape is placed in a zone of the
/// List row (<c>SummaryAdmin</c>) and in the column of the Grid (<c>SummaryAdminGrid</c>).
/// </summary>
public sealed class DataPipelineEntryDisplayDriver : DisplayDriver<DataPipelineEntry>
{
    public override Task<IDisplayResult> DisplayAsync(DataPipelineEntry entry, BuildDisplayContext context)
    {
        if (context.DisplayType is OrchardCoreConstants.DisplayType.SummaryAdmin or OrchardCoreConstants.DisplayType.SummaryAdminGrid
            && context.Shape is Shape row)
        {
            row.Attributes["data-pipeline-id"] = entry.Pipeline.PipelineId;
        }

        var results = new List<IDisplayResult>
        {
            View("DataPipelineEntry_Fields_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Content:1")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:1"),
            View("DataPipelineEntry_Description_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Description:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Name:5")
                .RenderWhen(() => Task.FromResult(!string.IsNullOrEmpty(entry.Pipeline.Description))),
            View("DataPipelineEntry_Status_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Tags:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Status:5"),
            View("DataPipelineEntry_LastRun_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "LastRun:5"),
            View("DataPipelineEntry_Modified_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Meta:10")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Modified:5"),
            View("DataPipelineEntry_Buttons_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "Actions:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "Actions:5"),
            View("DataPipelineEntry_ActionsMenuItems_SummaryAdmin", entry)
                .Location(OrchardCoreConstants.DisplayType.SummaryAdmin, "ActionsMenu:5")
                .Location(OrchardCoreConstants.DisplayType.SummaryAdminGrid, "ActionsMenu:5"),
        };

        return CombineAsync(results);
    }
}
