using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment
{
    public abstract class DeploymentPlanResultBase
    {
        public IList<JsonObject> Steps { get; init; } = [];

        public JsonObject Recipe { get; }

        abstract public Task FinalizeAsync();

        public DeploymentPlanResultBase(RecipeDescriptor recipeDescriptor)
        {
            Recipe = new JsonObject
            {
                ["name"] = recipeDescriptor.Name ?? string.Empty,
                ["displayName"] = recipeDescriptor.DisplayName ?? string.Empty,
                ["description"] = recipeDescriptor.Description ?? string.Empty,
                ["author"] = recipeDescriptor.Author ?? string.Empty,
                ["website"] = recipeDescriptor.WebSite ?? string.Empty,
                ["version"] = recipeDescriptor.Version ?? string.Empty,
                ["issetuprecipe"] = recipeDescriptor.IsSetupRecipe,
                ["categories"] = JArray.FromObject(recipeDescriptor.Categories ?? []),
                ["tags"] = JArray.FromObject(recipeDescriptor.Tags ?? []),
            };
        }
    }
}
