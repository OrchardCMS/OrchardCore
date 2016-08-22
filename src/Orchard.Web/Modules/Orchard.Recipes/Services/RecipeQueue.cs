using Orchard.FileSystem;
using Orchard.Recipes.Models;
using System.Threading.Tasks;

namespace Orchard.Recipes.Services
{
    public class RecipeQueue : IRecipeQueue
    {
        private readonly IRecipeParser _recipeParser;
        private readonly IRecipeStepQueue _recipeStepQueue;

        private readonly IOrchardFileSystem _fileSystem;

        public RecipeQueue(
            IRecipeParser recipeParser,
            IRecipeStepQueue recipeStepQueue,
            IOrchardFileSystem fileSystem)
        {
            _recipeParser = recipeParser;
            _recipeStepQueue = recipeStepQueue;

            _fileSystem = fileSystem;
        }

        public async Task<string> EnqueueAsync(string executionId, RecipeDescriptor recipeDescriptor)
        {
            return await Task.Run(() =>
            {
                _recipeParser.ProcessRecipe(
                    _fileSystem.GetFileInfo(recipeDescriptor.Location), async (recipe, recipeStep) =>
                    {
                        await _recipeStepQueue.EnqueueAsync(executionId, recipeStep);
                    });

                return executionId;
            });
        }
    }
}
