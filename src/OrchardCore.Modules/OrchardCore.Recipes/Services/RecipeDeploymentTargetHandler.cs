using System;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Deployment;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public class RecipeDeploymentTargetHandler : IDeploymentTargetHandler
    {
        private readonly IRecipeExecutor _recipeExecutor;

        public RecipeDeploymentTargetHandler(IRecipeExecutor recipeExecutor)
        {
            _recipeExecutor = recipeExecutor;
        }

        public async Task ImportFromFileAsync(IFileProvider fileProvider)
        {
            var executionId = Guid.NewGuid().ToString("n");
            var recipeDescriptor = new RecipeDescriptor
            {
                FileProvider = fileProvider,
                BasePath = "",
                RecipeFileInfo = fileProvider.GetFileInfo("Recipe.json")
            };

            await _recipeExecutor.ExecuteAsync(executionId, recipeDescriptor, new object());
        }
    }
}
