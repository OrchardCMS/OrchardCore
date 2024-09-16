using System.Text;
using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment;

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

    public JsonObject Recipe { get; }
    public IList<JsonObject> Steps { get; init; } = [];
    public IFileBuilder FileBuilder { get; }
    public async Task FinalizeAsync()
    {
        Recipe["steps"] = JArray.FromObject(Steps);

        // Add the recipe steps as its own file content
        await FileBuilder.SetFileAsync("Recipe.json", Encoding.UTF8.GetBytes(Recipe.ToString()));
    }
}
