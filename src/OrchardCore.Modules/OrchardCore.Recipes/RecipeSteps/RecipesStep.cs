using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.RecipeSteps
{
    /// <summary>
    /// This recipe step executes a set of external recipes.
    /// </summary>
    public class RecipesStep : IRecipeStepHandler
    {
        private readonly IEnumerable<IRecipeHarvester> _recipeHarvesters;

        public RecipesStep(IEnumerable<IRecipeHarvester> recipeHarvesters)
        {
            _recipeHarvesters = recipeHarvesters;
        }

        public async Task ExecuteAsync(RecipeExecutionContext context)
        {
            if (!String.Equals(context.Name, "Recipes", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var step = context.Step.ToObject<InternalStep>();
            var recipesDictionary = new Dictionary<string, IDictionary<string, RecipeDescriptor>>();
            IList<RecipeDescriptor> innerRecipes = new List<RecipeDescriptor>();

            foreach (var recipe in step.Values)
            {
                IDictionary<string, RecipeDescriptor> recipes;

                if (!recipesDictionary.TryGetValue(recipe.ExecutionId, out recipes))
                {
                    var recipeCollections = await Task.WhenAll(_recipeHarvesters.Select(x => x.HarvestRecipesAsync()));
                    recipes = recipeCollections.SelectMany(x => x).ToDictionary(x => x.Name);
                    recipesDictionary[recipe.ExecutionId] = recipes;
                }

                if (!recipes.ContainsKey(recipe.Name))
                {
                    throw new ArgumentException($"No recipe named '{recipe.Name}' was found in extension '{recipe.ExecutionId}'.");
                }

                innerRecipes.Add(recipes[recipe.Name]);
            }

            context.InnerRecipes = innerRecipes;
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
