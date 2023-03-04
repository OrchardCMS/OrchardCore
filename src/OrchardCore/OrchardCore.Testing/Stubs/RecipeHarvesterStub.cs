using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Testing.Recipes;

namespace OrchardCore.Testing.Stubs
{
    public class RecipeHarvesterStub : IRecipeHarvester
    {
        private readonly IRecipeFileProvider _recipeFileProvider;
        private readonly IRecipeReader _recipeReader;

        public RecipeHarvesterStub(IRecipeFileProvider recipeFileProvider, IRecipeReader recipeReader)
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
