using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public class RecipeReader : IRecipeReader
    {
        public async Task<RecipeDescriptor> GetRecipeDescriptorAsync(string recipeBasePath, IFileInfo recipeFileInfo, IFileProvider recipeFileProvider)
        {
            // TODO: Try to optimize by only reading the required metadata instead of the whole file.
            using var stream = recipeFileInfo.CreateReadStream();

            var recipeDescriptor = await JsonSerializer.DeserializeAsync<RecipeDescriptor>(stream, JOptions.Default);

            recipeDescriptor.FileProvider = recipeFileProvider;
            recipeDescriptor.BasePath = recipeBasePath;
            recipeDescriptor.RecipeFileInfo = recipeFileInfo;

            return recipeDescriptor;
        }
    }
}
