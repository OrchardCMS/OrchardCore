using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment;

public class MemoryDeploymentPlanResult : DeploymentPlanResult
{
    private readonly MemoryStream _stream;

    public MemoryDeploymentPlanResult(MemoryStream stream, RecipeDescriptor recipeDescriptor)
        : base(recipeDescriptor)
    {
        _stream = stream;
    }

    public override async Task FinalizeAsync()
    {
        Recipe["steps"] = JArray.FromObject(Steps);

        // Add the recipe steps as its own file content

        await _stream.WriteAsync(GetContent());
    }
}
