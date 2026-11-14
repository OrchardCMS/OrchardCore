using Microsoft.Extensions.Localization;
using OrchardCore.DataPipelines.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DataPipelines.Drivers;

/// <summary>
/// Builds the editor of the page adding a data pipeline, whose fields are grouped in cards by their placement.
/// </summary>
internal sealed class DataPipelineCreateDisplayDriver : DisplayDriver<DataPipelineCreateViewModel>
{
    internal readonly IStringLocalizer S;

    public DataPipelineCreateDisplayDriver(IStringLocalizer<DataPipelineCreateDisplayDriver> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(DataPipelineCreateViewModel model, BuildEditorContext context)
    {
        return Initialize<DataPipelineCreateViewModel>("DataPipelineCreateGeneral_Edit", m =>
        {
            m.Name = model.Name;
            m.Description = model.Description;
        }).Location(l => l.Zone("Content", "1").Card(S["General"], "10"));
    }

    public override async Task<IDisplayResult> UpdateAsync(DataPipelineCreateViewModel model, UpdateEditorContext context)
    {
        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.Name);

        return Edit(model, context);
    }

    // The fields keep the names the page posts, which the controller validates.
    protected override void BuildPrefix(DataPipelineCreateViewModel model, string htmlFieldPrefix)
    {
        Prefix = htmlFieldPrefix ?? string.Empty;
    }
}
