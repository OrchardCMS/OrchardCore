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
    private readonly IRuleManagementService _rules;
    private readonly IEnumerable<IConditionFactory> _factories;
    private readonly JsonSerializerOptions _serializationOptions;

    internal readonly IStringLocalizer S;

    public LayerStep(
        ILayerService layerService,
        IRuleManagementService rules,
        IEnumerable<IConditionFactory> factories,
        IOptions<DocumentJsonSerializerOptions> serializationOptions,
        IStringLocalizer<LayerStep> stringLocalizer)
        : base("Layers")
    {
        _layerService = layerService;
        _rules = rules;
        _factories = factories;
        _serializationOptions = serializationOptions.Value.SerializerOptions;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        // The recipe step contains polymorphic types which need to be resolved.
        var model = context.Step.ToObject<LayersStepModel>(_serializationOptions);

        var allLayers = await _layerService.GetLayersAsync();
        var prepared = new Dictionary<string, Layer>(StringComparer.OrdinalIgnoreCase);
        var unknownTypes = new List<string>();
        var factories = _factories.ToDictionary(x => x.Name);

        foreach (var layerStep in model.Layers)
        {
            if (_layerService.ValidateName(layerStep.Name) is { } error)
            {
                context.Errors.Add(error);
                continue;
            }

            var existing = prepared.GetValueOrDefault(layerStep.Name)
                ?? allLayers.Layers.FirstOrDefault(x => string.Equals(x.Name, layerStep.Name, StringComparison.OrdinalIgnoreCase));
            // Keep the immutable document untouched when a missing root identity is initialized.
            var rule = existing?.LayerRule is { } originalRule
                ? new Rule { Name = originalRule.Name, ConditionId = originalRule.ConditionId, Conditions = [.. originalRule.Conditions] }
                : null;
            if (layerStep.LayerRule is not null)
            {
                rule = new Rule
                {
                    ConditionId = string.IsNullOrEmpty(layerStep.LayerRule.ConditionId)
                        ? existing?.LayerRule?.ConditionId : layerStep.LayerRule.ConditionId,
                };

                // Recipe conditions support registered extensions beyond the public API contract.
                foreach (var jCondition in layerStep.LayerRule.Conditions ?? [])
                {
                    var name = jCondition?["Name"]?.ToString();
                    if (name is not null && factories.TryGetValue(name, out var factory))
                    {
                        var condition = (Condition)jCondition.ToObject(factory.Create().GetType(), _serializationOptions);
                        ValidateCondition(condition, context);
                        rule.Conditions.Add(condition);
                    }
                    else
                    {
                        unknownTypes.Add(name ?? string.Empty);
                    }
                }
            }

            prepared[layerStep.Name] = new Layer
            {
                Name = existing?.Name ?? layerStep.Name,
                Description = string.IsNullOrEmpty(layerStep.Description) ? existing?.Description : layerStep.Description,
                LayerRule = rule,
            };
        }

        if (unknownTypes.Count != 0)
        {
            context.Errors.Add(S["No changes have been made. The following types of conditions cannot be added: {0}. Please ensure that the related features are enabled to add these types of conditions.", string.Join(", ", unknownTypes)]);
        }

        if (context.Errors.Count != 0)
        {
            return;
        }

        foreach (var layer in prepared.Values)
        {
            var exists = allLayers.Layers.Any(x => string.Equals(x.Name, layer.Name, StringComparison.OrdinalIgnoreCase));
            var result = exists
                ? await _layerService.UpdateAsync(layer.Name, layer.Description, layer.LayerRule)
                : await _layerService.CreateAsync(layer.Name, layer.Description, layer.LayerRule);
            if (result.Status != LayerMutationStatus.Success)
            {
                context.Errors.Add(result.Error ?? S["The layer '{0}' could not be updated.", layer.Name]);
                return;
            }
        }
    }

    private void ValidateCondition(Condition condition, RecipeExecutionContext context)
    {
        foreach (var error in _rules.ValidateCondition(condition))
        {
            context.Errors.Add(error.Message);
        }

        if (condition is ConditionGroup group)
        {
            foreach (var child in group.Conditions)
            {
                ValidateCondition(child, context);
            }
        }
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
