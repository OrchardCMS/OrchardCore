using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Services
{
    public class UpdateRecipeExecutionStepContext
    {
        public JsonObject RecipeDocument { get; set; }
        public JsonObject Step { get; set; }
    }
}
