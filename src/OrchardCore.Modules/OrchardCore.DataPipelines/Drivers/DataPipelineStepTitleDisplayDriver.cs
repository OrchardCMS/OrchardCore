using OrchardCore.DataPipelines.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DataPipelines.Drivers;

/// <summary>
/// Edits the title of any step, shown on the designer canvas instead of the name of its type.
/// </summary>
public sealed class DataPipelineStepTitleDisplayDriver : DisplayDriver<DataPipelineStep>
{
    public override IDisplayResult Edit(DataPipelineStep step, BuildEditorContext context)
        => Initialize<DataPipelineStepTitleViewModel>("DataPipelineStepTitle_Edit", model => model.Title = step.Title)
            .Location("Content:0");

    public override async Task<IDisplayResult> UpdateAsync(DataPipelineStep step, UpdateEditorContext context)
    {
        var model = new DataPipelineStepTitleViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        step.Title = string.IsNullOrWhiteSpace(model.Title) ? null : model.Title.Trim();

        return Edit(step, context);
    }
}

public class DataPipelineStepTitleViewModel
{
    public string Title { get; set; }
}
