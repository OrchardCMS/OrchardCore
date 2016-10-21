using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orchard.Recipes.Models;

namespace Orchard.Recipes.Services
{
    public class JsonRecipeParser : IRecipeParser
    {
        public RecipeDescriptor ParseRecipe(Stream recipeStream)
        {
            var serializer = new JsonSerializer();

            StreamReader streamReader = new StreamReader(recipeStream);
            JsonTextReader reader = new JsonTextReader(streamReader);
            return serializer.Deserialize<RecipeDescriptor>(reader);
        }

        public Task ProcessRecipeAsync(
            Stream recipeStream,
            Func<RecipeDescriptor, RecipeStepDescriptor, Task> stepActionAsync)
        {
            var descriptor = ParseRecipe(recipeStream);

            recipeStream.Position = 0;
            return ParseStepsAsync(recipeStream, descriptor, stepActionAsync);
        }

        private async Task ParseStepsAsync(
            Stream stream,
            RecipeDescriptor descriptor,
            Func<RecipeDescriptor, RecipeStepDescriptor, Task> stepActionAsync)
        {
            var serializer = new JsonSerializer();

            StreamReader streamReader = new StreamReader(stream);
            JsonTextReader reader = new JsonTextReader(streamReader);

            // Go to Steps, then iterate.
            while (reader.Read())
            {
                if (reader.Path == "steps" && reader.TokenType == JsonToken.StartArray)
                {
                    int stepId = 0;
                    while (reader.Read() && reader.Depth > 1)
                    {
                        if (reader.Depth == 2)
                        {
                            var child = JToken.Load(reader);
                            await stepActionAsync(descriptor, new RecipeStepDescriptor
                            {
                                Id = (stepId++).ToString(CultureInfo.InvariantCulture),
                                Name = child.Value<string>("name"),
                                Step = child
                            });
                        }
                    }
                }
            }
        }
    }
}