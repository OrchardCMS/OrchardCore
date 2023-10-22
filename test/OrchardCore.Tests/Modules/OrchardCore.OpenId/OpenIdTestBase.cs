using OrchardCore.Recipes.Models;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Utilities;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

public class OpenIdTestBase
{

    protected string GetRecipeFileContent(string recipeName) =>
        new EmbeddedFileProvider(GetType().Assembly)
            .GetFileInfo($"Modules.OrchardCore.OpenId.RecipeFiles.{recipeName}.json")
            .ReadToEnd();

    protected RecipeExecutionContext GetRecipeExecutionContext(JsonNode recipe) =>
        new()
        {
            Name = recipe.GetNode("steps", 0, "name").GetValue<string>(),
            Step = recipe.GetNode("steps", 0).AsObject(),
        };

    protected RecipeExecutionContext GetRecipeExecutionContext(string recipeName) =>
        GetRecipeExecutionContext(JsonSerializer.SerializeToNode(GetRecipeFileContent(recipeName)));
}
