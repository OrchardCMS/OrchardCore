using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Orchard.Recipes.Models;
using System.IO;

namespace Orchard.Recipes.Services
{
    public class RecipeParser : IRecipeParser
    {
        public RecipeDescriptor ParseRecipe(IFileInfo recipeFile)
        {
            var serializer = new JsonSerializer();

            using (StreamReader streamReader = new StreamReader(recipeFile.CreateReadStream()))
            {
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    return serializer.Deserialize<RecipeDescriptor>(reader);
                }
            }
        }
    }
}
