using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.Recipes.RecipeSteps
{
    public class RecipesStep : RecipeExecutionStep
    {
        private readonly IRecipeHarvester _recipeHarvester;
        private readonly IRecipeExecutor _recipeManager;

        public RecipesStep(
            IRecipeHarvester recipeHarvester,
            IRecipeExecutor recipeManager,
            ILogger<RecipesStep> logger,
            IStringLocalizer<RecipesStep> localizer) : base(logger, localizer)
        {

            _recipeHarvester = recipeHarvester;
            _recipeManager = recipeManager;
        }

        public override string Name => "Recipes";

        /*
         {
            "name": "recipes",
            "recipes": [
                { "executionid": "Orchard.Setup", name="Core" }
            ]
         }
        */
        public override async Task ExecuteAsync(RecipeExecutionContext context)
        {
            var step = context.RecipeStep.Step.ToObject<InternalStep>();
            var recipesDictionary = new Dictionary<string, IDictionary<string, RecipeDescriptor>>();

            foreach (var recipe in step.Values)
            {
                Logger.LogInformation("Executing recipe '{0}' in extension '{1}'.", recipe.Name, recipe.ExecutionId);

                try
                {
                    var recipes = recipesDictionary.ContainsKey(recipe.ExecutionId) ? recipesDictionary[recipe.ExecutionId] : default(IDictionary<string, RecipeDescriptor>);
                    if (recipes == null)
                    {
                        recipes = recipesDictionary[recipe.ExecutionId] = await HarvestRecipes(recipe.ExecutionId);
                    }

                    if (!recipes.ContainsKey(recipe.Name))
                    {
                        throw new Exception(string.Format("No recipe named '{0}' was found in extension '{1}'.", recipe.Name, recipe.ExecutionId));
                    }

                    await _recipeManager.ExecuteAsync(context.ExecutionId, recipes[recipe.Name]);
                }
                catch
                {
                    Logger.LogError("Error while executing recipe '{0}' in extension '{1}'.", recipe.Name, recipe.ExecutionId);
                    throw;
                }
            }
        }

        private async Task<IDictionary<string, RecipeDescriptor>> HarvestRecipes(string extensionId)
        {
            try
            {
                var recipes = await _recipeHarvester.HarvestRecipesAsync(extensionId);
                return recipes.ToDictionary(x => x.Name);
            }
            catch (ArgumentException ex)
            {
                throw new OrchardFatalException(T["A recipe with the same name has been detected for extension \"{0}\". Please make sure recipes are uniquely named.", extensionId], ex);
            }
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
