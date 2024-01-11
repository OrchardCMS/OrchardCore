using System;
using System.Collections.Generic;
using System.Text;
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

            Recipe = new JObject
            {
                ["name"] = recipeDescriptor.Name ?? "",
                ["displayName"] = recipeDescriptor.DisplayName ?? "",
                ["description"] = recipeDescriptor.Description ?? "",
                ["author"] = recipeDescriptor.Author ?? "",
                ["website"] = recipeDescriptor.WebSite ?? "",
                ["version"] = recipeDescriptor.Version ?? "",
                ["issetuprecipe"] = recipeDescriptor.IsSetupRecipe,
                ["categories"] = new JArray(recipeDescriptor.Categories ?? Array.Empty<string>()),
                ["tags"] = new JArray(recipeDescriptor.Tags ?? Array.Empty<string>()),
            };
        }

        public JObject Recipe { get; }
        public IList<JObject> Steps { get; } = new List<JObject>();
        public IFileBuilder FileBuilder { get; }
        public async Task FinalizeAsync()
        {
            Recipe["steps"] = new JArray(Steps);

            // Add the recipe steps as its own file content
            await FileBuilder.SetFileAsync("Recipe.json", Encoding.UTF8.GetBytes(Recipe.ToString(Formatting.Indented)));
        }
    }
}
