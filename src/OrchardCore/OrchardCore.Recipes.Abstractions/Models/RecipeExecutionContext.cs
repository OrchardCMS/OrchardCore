using System.Text.Json.Nodes;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Recipes.Models;

public class RecipeExecutionContext
{
    public string ExecutionId { get; set; }

    public object Environment { get; set; }

    public string Name { get; set; }

    public JsonObject Step { get; set; }

    public IRecipeDescriptor RecipeDescriptor { get; set; }

    public IEnumerable<IRecipeDescriptor> InnerRecipes { get; set; }

    public IList<string> Errors { get; } = [];
}
