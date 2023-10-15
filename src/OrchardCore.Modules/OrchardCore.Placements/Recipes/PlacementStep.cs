using System.Threading.Tasks;
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

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (context.TryGetStepPropertyIfNameMatches<PlacementNode[]>("Placements", out var templates))
            {
                foreach (var (name, value) in templates)
                {
                    await _placementsManager.UpdateShapePlacementsAsync(name, value);
                }
            }
        }
    }
}
