using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Services;

public sealed class RecipeSchema
{
    public string Name { get; set; }

    public JsonObject Data { get; set; }
}
