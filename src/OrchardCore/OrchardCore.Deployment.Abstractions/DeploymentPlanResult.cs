using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment
{
    /// <summary>
    /// The state of a deployment plan built by sources.
    /// </summary>
    public class DeploymentPlanResult
    {
        public DeploymentPlanResult(IFileBuilder fileBuilder, RecipeDescriptor recipeDescriptor)
        {
            FileBuilder = fileBuilder;

            Recipe = new JsonObject
            {
                ["name"] = recipeDescriptor.Name ?? "",
                ["displayName"] = recipeDescriptor.DisplayName ?? "",
                ["description"] = recipeDescriptor.Description ?? "",
                ["author"] = recipeDescriptor.Author ?? "",
                ["website"] = recipeDescriptor.WebSite ?? "",
                ["version"] = recipeDescriptor.Version ?? "",
                ["issetuprecipe"] = recipeDescriptor.IsSetupRecipe,
                ["categories"] = new JsonArray(recipeDescriptor.Categories ?? Array.Empty<string>()),
                ["tags"] = new JsonArray(recipeDescriptor.Tags ?? Array.Empty<string>()),
            };
        }

        public JsonObject Recipe { get; }
        public IList<JsonObject> Steps { get; } = new List<JsonObject>();
        public IFileBuilder FileBuilder { get; }
        public async Task FinalizeAsync()
        {
            Recipe["steps"] = new JArray(Steps);

            // Add the recipe steps as its own file content
            await FileBuilder.SetFileAsync("Recipe.json", Encoding.UTF8.GetBytes(Recipe.ToString(Formatting.Indented)));
        }
    }
}
