using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.DataPipelines.Recipes;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Deployment;
using OrchardCore.Json;

namespace OrchardCore.DataPipelines.Deployment;

/// <summary>
/// Exports every data pipeline as a <see cref="DataPipelinesRecipeStep"/>, without the secrets of its steps, which
/// can't be read on another site.
/// </summary>
public sealed class AllDataPipelinesDeploymentSource : DeploymentSourceBase<AllDataPipelinesDeploymentStep>
{
    private readonly DataPipelineManager _pipelineManager;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public AllDataPipelinesDeploymentSource(DataPipelineManager pipelineManager, IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
    {
        _pipelineManager = pipelineManager;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    protected override async Task ProcessAsync(AllDataPipelinesDeploymentStep step, DeploymentPlanResult result)
    {
        var pipelines = new JsonArray();

        foreach (var pipeline in await _pipelineManager.ListAsync())
        {
            var definition = JObject.FromObject(pipeline.Published ?? pipeline.GetEditableDefinition(), _jsonSerializerOptions);
            DataPipelineSecrets.RemoveProtectedValues(definition);

            pipelines.Add(new JsonObject
            {
                [nameof(DataPipelineRecipeModel.PipelineId)] = pipeline.PipelineId,
                [nameof(DataPipelineRecipeModel.Name)] = pipeline.Name,
                [nameof(DataPipelineRecipeModel.Description)] = pipeline.Description,
                [nameof(DataPipelineRecipeModel.IsEnabled)] = pipeline.IsEnabled,
                [nameof(DataPipelineRecipeModel.Definition)] = definition,
            });
        }

        result.Steps.Add(new JsonObject
        {
            ["name"] = DataPipelinesRecipeStep.RecipeStepName,
            [nameof(DataPipelinesStepModel.Pipelines)] = pipelines,
        });
    }
}
