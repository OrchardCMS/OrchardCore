using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services
{
    public class RecipeReader : IRecipeReader
    {
        public async Task<RecipeDescriptor> GetRecipeDescriptorAsync(string recipeBasePath, IFileInfo recipeFileInfo, IFileProvider recipeFileProvider)
        {
            // TODO: Try to optimize by only reading the required metadata instead of the whole file.

            using var stream = recipeFileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(reader);

            var recipeDescriptor = (await JObject.LoadAsync(jsonReader)).ToObject<RecipeDescriptor>();

            recipeDescriptor.FileProvider = recipeFileProvider;
            recipeDescriptor.BasePath = recipeBasePath;
            recipeDescriptor.RecipeFileInfo = recipeFileInfo;

            return recipeDescriptor;
        }
    }
}
