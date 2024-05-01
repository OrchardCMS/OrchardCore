using System.IO;
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

    protected override async Task WriteAsync()
    {
        // Add the recipe steps as its own file content
        await _stream.WriteAsync(GetContent());
    }
}
