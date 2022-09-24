using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Placements.Services;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Placements.Recipes
{
    /// <summary>
    /// This recipe step creates a set of placements.
    /// </summary>
    public class PlacementStep : IRecipeStepHandler
    {
        private readonly PlacementsManager _placementsManager;

        public PlacementStep(PlacementsManager placementsManager)
        {
            _placementsManager = placementsManager;
        }

        public int Order => 0;

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Placements", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (context.Step.Property("Placements").Value is JObject templates)
            {
                foreach (var property in templates.Properties())
                {
                    var name = property.Name;
                    var value = property.Value.ToObject<PlacementNode[]>();

                    await _placementsManager.UpdateShapePlacementsAsync(name, value);
                }
            }
        }
    }
}
