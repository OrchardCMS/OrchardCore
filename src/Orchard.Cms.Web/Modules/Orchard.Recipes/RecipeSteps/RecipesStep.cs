using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;

namespace Orchard.Recipes.RecipeSteps
{
    /// <summary>
    /// This recipe step executes a set of external recipes.
    /// </summary>
    public class RecipesStep : IRecipeStepHandler
    {
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IRecipeExecutor _recipeManager;

        public RecipesStep(
            IRecipeHarvester recipeHarvester,
            IRecipeExecutor recipeManager)
        {
            _recipeHarvester = recipeHarvester;
            _recipeManager = recipeManager;
        }

        /*
         {
            "name": "recipes",
            "recipes": [
                { "executionid": "Orchard.Setup", name="Core" }
            ]
         }
        */
        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Recipes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var step = context.Step.ToObject<InternalStep>();
            var recipesDictionary = new Dictionary<string, IDictionary<string, RecipeDescriptor>>();

            foreach (var recipe in step.Values)
            {
                var recipes = recipesDictionary.ContainsKey(recipe.ExecutionId) ? recipesDictionary[recipe.ExecutionId] : default(IDictionary<string, RecipeDescriptor>);
                if (recipes == null)
                {
                    recipes = recipesDictionary[recipe.ExecutionId] = await HarvestRecipes(recipe.ExecutionId);
                }

                if (!recipes.ContainsKey(recipe.Name))
                {
                    throw new ArgumentException($"No recipe named '{recipe.Name}' was found in extension '{recipe.ExecutionId}'.");
                }

                var executionId = Guid.NewGuid().ToString();
                await _recipeManager.ExecuteAsync(executionId, recipes[recipe.Name], context.Environment);
            }
        }

        private async Task<IDictionary<string, RecipeDescriptor>> HarvestRecipes(string extensionId)
        {
            var recipes = await _recipeHarvester.HarvestRecipesAsync(extensionId);
            return recipes.ToDictionary(x => x.Name);
        }

        private class InternalStep
        {
            public InternalStepValue[] Values { get; set; }
        }

        private class InternalStepValue
        {
            public string ExecutionId { get; set; }
            public string Name { get; set; }
        }
    }
}
