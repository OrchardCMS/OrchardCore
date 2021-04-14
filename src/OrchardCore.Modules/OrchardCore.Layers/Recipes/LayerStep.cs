using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Rules;

namespace OrchardCore.Layers.Recipes
{
    /// <summary>
    /// This recipe step creates or updates a layer.
    /// </summary>
    public class LayerStep : IRecipeStepHandler
    {
        private readonly ILayerService _layerService;
        private readonly IRuleMigrator _ruleMigrator;
        private readonly IConditionIdGenerator _conditionIdGenerator;
        private readonly IEnumerable<IConditionFactory> _factories;

        public LayerStep(
            ILayerService layerService,
            IRuleMigrator ruleMigrator,
            IConditionIdGenerator conditionIdGenerator,
            IEnumerable<IConditionFactory> factories)
        {
            _layerService = layerService;
            _ruleMigrator = ruleMigrator;
            _conditionIdGenerator = conditionIdGenerator;
            _factories = factories;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Layers", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<LayerStepModel>();

            var allLayers = await _layerService.GetLayersAsync();

            var unknownTypes = new List<string>();
            var factories = _factories.ToDictionary(x => x.Name);

            foreach (var layerStep in model.Layers)
            {
                var layer = allLayers.Layers.FirstOrDefault(x => String.Equals(x.Name, layerStep.Name, StringComparison.OrdinalIgnoreCase));

                if (layer == null)
                {
                    layer = new Layer();
                    allLayers.Layers.Add(layer);
                }

                // Backwards compatability check.
                if (layer.LayerRule == null)
                {
                    layer.LayerRule = new Rule();
                    _conditionIdGenerator.GenerateUniqueId(layer.LayerRule);
                }

                // Replace any property that is set in the recipe step
                if (!String.IsNullOrEmpty(layerStep.Name))
                {
                    layer.Name = layerStep.Name;
                }
                else
                {
                    throw new ArgumentNullException($"{nameof(layer.Name)} is required");
                }

                if (layerStep.LayerRule != null)
                {
                    if (!String.IsNullOrEmpty(layerStep.LayerRule.ConditionId))
                    {
                        layer.LayerRule.ConditionId = layerStep.LayerRule.ConditionId;
                    }

                    // The conditions list is cleared, because we cannot logically merge conditions.
                    layer.LayerRule.Conditions.Clear();
                    foreach (var condition in layerStep.LayerRule.Conditions)
                    {
                        if (factories.TryGetValue(condition.Name, out var factory))
                        {
                            var jCondition = JObject.FromObject(condition);
                            var factoryCondition = (Condition)jCondition.ToObject(factory.Create().GetType());

                            layer.LayerRule.Conditions.Add(factoryCondition);
                        }
                        else
                        {
                             unknownTypes.Add(condition.Name);
                        }
                    }
                }

#pragma warning disable 0618
                // Migrate any old rule in a recipe to the new rule format.
                // Do not import the old rule.
                if (!String.IsNullOrEmpty(layerStep.Rule))
                {
                    _ruleMigrator.Migrate(layerStep.Rule, layer.LayerRule);
                }
#pragma warning restore 0618

                if (!String.IsNullOrEmpty(layerStep.Description))
                {
                    layer.Description = layerStep.Description;
                }
            }

            if (unknownTypes.Count != 0)
            {
                var prefix = "No changes have been made. The following types of conditions cannot be added:";
                var suffix = "Please ensure that the related features are enabled to add these types of conditions.";

                throw new InvalidOperationException($"{prefix} {String.Join(", ", unknownTypes)}. {suffix}");
            }

            await _layerService.UpdateAsync(allLayers);
        }
    }

    public class LayerStepModel
    {
        public Layer[] Layers { get; set; }
    }
}
