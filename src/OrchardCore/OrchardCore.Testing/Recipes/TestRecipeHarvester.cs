using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Testing.Recipes
{
    public class TestRecipeHarvester : IRecipeHarvester
    {
        private readonly IRecipeFileProvider _recipeFileProvider;
        private readonly IRecipeReader _recipeReader;

        public TestRecipeHarvester(IRecipeFileProvider recipeFileProvider, IRecipeReader recipeReader)
        {
            _recipeFileProvider = recipeFileProvider;
            _recipeReader = recipeReader;
        }

        public async Task<IEnumerable<RecipeDescriptor>> HarvestRecipesAsync()
        {
            var recipeFiles = _recipeFileProvider.GetRecipes();

            var recipeDescriptors = new List<RecipeDescriptor>();
            foreach (var recipeFile in recipeFiles)
            {
                if (recipeFile.Exists)
                {
                    var recipeDescriptor = await _recipeReader.GetRecipeDescriptor(
                        recipeFile.PhysicalPath,
                        recipeFile,
                        _recipeFileProvider.FileProvider);

                    recipeDescriptors.Add(recipeDescriptor);
                }
            }

            return recipeDescriptors;
        }
    }
}
