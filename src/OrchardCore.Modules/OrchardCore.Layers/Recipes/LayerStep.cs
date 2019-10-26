using System;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Layers.Recipes
{
    /// <summary>
    /// This recipe step creates or updates a layer.
    /// </summary>
    public class LayerStep : IRecipeStepHandler
    {
        private readonly ILayerService _layerService;

        public LayerStep(ILayerService layerService)
        {
            _layerService = layerService;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Layers", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var model = context.Step.ToObject<LayerStepModel>();

            var allLayers = await _layerService.LoadLayersAsync();

            foreach (Layer layer in model.Layers)
            {
                var existing = allLayers.Layers.FirstOrDefault(x => String.Equals(x.Name, layer.Name, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    // Replace any property that is set in the recipe step
                    if (!String.IsNullOrEmpty(layer.Rule))
                    {
                        existing.Rule = layer.Rule;
                    }

                    if (!String.IsNullOrEmpty(layer.Description))
                    {
                        existing.Description = layer.Description;
                    }
                }
                else
                {
                    allLayers.Layers.Add(layer);
                }
            }

            await _layerService.UpdateAsync(allLayers);
        }
    }

    public class LayerStepModel
    {
        public Layer[] Layers { get; set; }
    }
}
