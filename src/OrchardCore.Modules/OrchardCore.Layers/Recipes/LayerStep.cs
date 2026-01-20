using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Json;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Rules;
using OrchardCore.Rules.Services;

namespace OrchardCore.Layers.Recipes;

/// <summary>
/// This recipe step creates or updates a layer.
/// </summary>
public sealed class LayerStep : NamedRecipeStepHandler
{
    private readonly ILayerService _layerService;
    private readonly IConditionIdGenerator _conditionIdGenerator;
    private readonly IEnumerable<IConditionFactory> _factories;
    private readonly JsonSerializerOptions _serializationOptions;

    internal readonly IStringLocalizer S;

    public LayerStep(
        ILayerService layerService,
        IConditionIdGenerator conditionIdGenerator,
        IEnumerable<IConditionFactory> factories,
        IOptions<DocumentJsonSerializerOptions> serializationOptions,
        IStringLocalizer<LayerStep> stringLocalizer)
        : base("Layers")
    {
        _layerService = layerService;
        _conditionIdGenerator = conditionIdGenerator;
        _factories = factories;
        _serializationOptions = serializationOptions.Value.SerializerOptions;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        // The recipe step contains polymorphic types which need to be resolved.
        var model = context.Step.ToObject<LayersStepModel>(_serializationOptions);

        var allLayers = await _layerService.LoadLayersAsync();

        var unknownTypes = new List<string>();
        var factories = _factories.ToDictionary(x => x.Name);

        foreach (var layerStep in model.Layers)
        {
            var layer = allLayers.Layers.FirstOrDefault(x => string.Equals(x.Name, layerStep.Name, StringComparison.OrdinalIgnoreCase));

            if (layer == null)
            {
                layer = new Layer();
                allLayers.Layers.Add(layer);
            }

            // Backwards compatibility check.
            if (layer.LayerRule == null)
            {
                layer.LayerRule = new Rule();
                _conditionIdGenerator.GenerateUniqueId(layer.LayerRule);
            }

            // Replace any property that is set in the recipe step.
            if (!string.IsNullOrEmpty(layerStep.Name))
            {
                layer.Name = layerStep.Name;
            }
            else
            {
                context.Errors.Add(S["The layer '{0}' is required.", layer.Name]);

                continue;
            }

            if (layerStep.LayerRule != null)
            {
                if (!string.IsNullOrEmpty(layerStep.LayerRule.ConditionId))
                {
                    layer.LayerRule.ConditionId = layerStep.LayerRule.ConditionId;
                }

                // The conditions list is cleared, because we cannot logically merge conditions.
                layer.LayerRule.Conditions.Clear();
                foreach (var jCondition in layerStep.LayerRule.Conditions)
                {
                    var name = jCondition["Name"].ToString();
                    if (factories.TryGetValue(name, out var factory))
                    {
                        var factoryCondition = (Condition)jCondition.ToObject(factory.Create().GetType(), _serializationOptions);

                        layer.LayerRule.Conditions.Add(factoryCondition);
                    }
                    else
                    {
                        unknownTypes.Add(name);
                    }
                }
            }

            if (!string.IsNullOrEmpty(layerStep.Description))
            {
                layer.Description = layerStep.Description;
            }
        }

        if (unknownTypes.Count != 0)
        {
            context.Errors.Add(S["No changes have been made. The following types of conditions cannot be added: {0}. Please ensure that the related features are enabled to add these types of conditions.", string.Join(", ", unknownTypes)]);

            return;
        }

        await _layerService.UpdateAsync(allLayers);
    }
}

public class LayersStepModel
{
    public LayerStepModel[] Layers { get; set; }
}

public class LayerStepModel
{
    public string Name { get; set; }

    public string Rule { get; set; }
    public string Description { get; set; }

    public RuleStepModel LayerRule { get; set; }
}

public class RuleStepModel
{
    public string Name { get; set; }
    public string ConditionId { get; set; }
    public JsonArray Conditions { get; set; }
}
