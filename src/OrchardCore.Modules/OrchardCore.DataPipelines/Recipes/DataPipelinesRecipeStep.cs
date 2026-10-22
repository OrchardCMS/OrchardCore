using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Json;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.DataPipelines.Recipes;

/// <summary>
/// Imports data pipelines. A pipeline is matched by its identifier: an existing one gets the imported name, description
/// and definition as its draft, and keeps its published version; a new one is created with the definition as its draft.
/// An imported definition is never published, as runs execute with the access of the user who publishes the pipeline.
/// </summary>
public sealed class DataPipelinesRecipeStep : NamedRecipeStepHandler
{
    /// <summary>
    /// The name of the recipe step.
    /// </summary>
    public const string RecipeStepName = "DataPipelines";

    private readonly DataPipelineManager _pipelineManager;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IStringLocalizer S;

    public DataPipelinesRecipeStep(
        DataPipelineManager pipelineManager,
        IOptions<DocumentJsonSerializerOptions> jsonSerializerOptions,
        IStringLocalizer<DataPipelinesRecipeStep> localizer)
        : base(RecipeStepName)
    {
        _pipelineManager = pipelineManager;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
        S = localizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        var model = context.Step.ToObject<DataPipelinesStepModel>(_jsonSerializerOptions);

        foreach (var item in model?.Pipelines ?? [])
        {
            if (string.IsNullOrWhiteSpace(item.Name))
            {
                context.Errors.Add(S["The data pipeline '{0}' has no name.", item.PipelineId]);

                continue;
            }

            if (item.PipelineId?.Trim().Length > DataPipelineIndex.MaxIdLength)
            {
                context.Errors.Add(S["The identifier of the data pipeline '{0}' is longer than {1} characters.", item.Name, DataPipelineIndex.MaxIdLength]);

                continue;
            }

            var pipeline = string.IsNullOrWhiteSpace(item.PipelineId) ? null : await _pipelineManager.GetAsync(item.PipelineId.Trim());

            if (pipeline is null)
            {
                pipeline = await _pipelineManager.CreateAsync(item.Name, item.Description, user: null, item.PipelineId);
            }
            else
            {
                await _pipelineManager.UpdateSettingsAsync(pipeline, item.Name, item.Description);
            }

            pipeline.IsEnabled = item.IsEnabled;

            var definition = item.Definition ?? new DataPipelineDefinition();

            await _pipelineManager.SaveDraftAsync(pipeline, pipeline.Revision, draft =>
            {
                draft.Steps = definition.Steps ?? [];
                draft.Connections = definition.Connections ?? [];
            }, user: null);
        }
    }
}

/// <summary>
/// The <c>DataPipelines</c> recipe step.
/// </summary>
public sealed class DataPipelinesStepModel
{
    /// <summary>
    /// Gets or sets the pipelines to import.
    /// </summary>
    public List<DataPipelineRecipeModel> Pipelines { get; set; } = [];
}

/// <summary>
/// A data pipeline in a recipe.
/// </summary>
public sealed class DataPipelineRecipeModel
{
    /// <summary>
    /// Gets or sets the identifier of the pipeline, at most 26 characters. An empty one creates a pipeline.
    /// </summary>
    public string PipelineId { get; set; }

    /// <summary>
    /// Gets or sets the name of the pipeline.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the pipeline.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the pipeline can run. Defaults to <see langword="true"/>.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the steps and connections of the pipeline.
    /// </summary>
    public DataPipelineDefinition Definition { get; set; }
}
