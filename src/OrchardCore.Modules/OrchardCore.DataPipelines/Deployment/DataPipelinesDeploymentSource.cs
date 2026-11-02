using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.DataPipelines.Recipes;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Deployment;
using OrchardCore.Json;

namespace OrchardCore.DataPipelines.Deployment;

/// <summary>
/// Exports data pipelines as a <see cref="DataPipelinesRecipeStep"/>, without the secrets of their steps, which can't
/// be read on another site.
/// </summary>
public sealed class DataPipelinesDeploymentSource : DeploymentSourceBase<DataPipelinesDeploymentStep>
{
    private readonly DataPipelineManager _pipelineManager;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public DataPipelinesDeploymentSource(DataPipelineManager pipelineManager, IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions)
    {
        _pipelineManager = pipelineManager;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    protected override async Task ProcessAsync(DataPipelinesDeploymentStep step, DeploymentPlanResult result)
    {
        var selected = new HashSet<string>(step.PipelineIds ?? [], StringComparer.Ordinal);
        var pipelines = new JsonArray();

        foreach (var pipeline in await _pipelineManager.ListAsync())
        {
            if (!step.IncludeAll && !selected.Contains(pipeline.PipelineId))
            {
                continue;
            }

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
