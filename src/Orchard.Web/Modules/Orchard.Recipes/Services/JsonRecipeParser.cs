using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Recipes.Models;
using System;
using System.IO;

namespace Orchard.Recipes.Services
{
    public class JsonRecipeParser : IRecipeParser
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

        public void ProcessRecipe(
            IFileInfo recipeFile, 
            Action<RecipeDescriptor, RecipeStepDescriptor> action)
        {
            var descriptor = ParseRecipe(recipeFile);

            var serializer = new JsonSerializer();

            using (StreamReader streamReader = new StreamReader(recipeFile.CreateReadStream()))
            {
                using (JsonTextReader reader = new JsonTextReader(streamReader))
                {
                    // Go to Steps, then iterate.
                    while (reader.Read()) {
                        if (reader.Path == "steps" && reader.TokenType == JsonToken.StartArray)
                        {
                            // Start Array
                            Console.WriteLine(reader.Value);
                            
                            action(descriptor, new RecipeStepDescriptor
                            {
                                RecipeName = descriptor.Name,
                                Name = "foo",
                                Step = JToken.Load(reader)
                            });
                        }
                    }
                }
            }
        }
    }
}
