using OrchardCore.DataPipelines.Services;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.DataPipelines.Deployment;

public sealed class DataPipelinesDeploymentStepDriver : DisplayDriver<DeploymentStep, DataPipelinesDeploymentStep>
{
    private readonly DataPipelineManager _pipelineManager;

    public DataPipelinesDeploymentStepDriver(DataPipelineManager pipelineManager)
    {
        _pipelineManager = pipelineManager;
    }

    public override Task<IDisplayResult> DisplayAsync(DataPipelinesDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("DataPipelinesDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("DataPipelinesDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content"));
    }

    public override IDisplayResult Edit(DataPipelinesDeploymentStep step, BuildEditorContext context)
    {
        return Initialize<DataPipelinesDeploymentStepViewModel>("DataPipelinesDeploymentStep_Fields_Edit", async model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.PipelineIds = step.PipelineIds ?? [];
            model.Pipelines = (await _pipelineManager.ListAsync())
                .Select(pipeline => new DataPipelineOption(pipeline.PipelineId, pipeline.Name))
                .ToList();
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(DataPipelinesDeploymentStep step, UpdateEditorContext context)
    {
        var model = new DataPipelinesDeploymentStepViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix, m => m.IncludeAll, m => m.PipelineIds);

        step.IncludeAll = model.IncludeAll;
        step.PipelineIds = model.IncludeAll ? [] : model.PipelineIds ?? [];

        return Edit(step, context);
    }
}

public class DataPipelinesDeploymentStepViewModel
{
    public bool IncludeAll { get; set; }

    public string[] PipelineIds { get; set; } = [];

    public IReadOnlyList<DataPipelineOption> Pipelines { get; set; } = [];
}

public sealed record DataPipelineOption(string PipelineId, string Name);
