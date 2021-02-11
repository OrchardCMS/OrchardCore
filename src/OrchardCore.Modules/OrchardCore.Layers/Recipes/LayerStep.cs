using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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

            var model = context.Step.ToObject<LayersStepModel>();

            var allLayers = await _layerService.LoadLayersAsync();

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

                if (!String.IsNullOrEmpty(layerStep.LayerRule.ConditionId))
                {
                    layer.LayerRule.ConditionId = layerStep.LayerRule.ConditionId;
                }

                if (layerStep.LayerRule != null)
                {
                    // The conditions list is cleared, because we cannot logically merge conditions.
                    layer.LayerRule.Conditions.Clear();
                    foreach (var conditionModel in layerStep.LayerRule.Conditions)
                    {
                        if (factories.TryGetValue(conditionModel.Name, out var factory))
                        {
                            var condition = (Condition)conditionModel.Condition.ToObject(factory.Create().GetType());

                            layer.LayerRule.Conditions.Add(condition);
                        }
                        else
                        {
                             unknownTypes.Add(conditionModel.Name);
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
        public string ConditionId { get; set; }
        public ConditionStepModel[] Conditions { get; set; }
    }

    public class ConditionStepModel
    {
        public string Name { get; set; }
        public JObject Condition { get; set; }
    }
}
