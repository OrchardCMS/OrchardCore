using System.Threading.Tasks;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Deployment
{
    /// <summary>
    /// The state of a deployment plan built by sources.
    /// </summary>
    public class DeploymentPlanResult : DeploymentPlanResultBase
    {
        protected DeploymentPlanResult(RecipeDescriptor recipeDescriptor)
            : base(recipeDescriptor)
        {
        }

        public DeploymentPlanResult(IFileBuilder fileBuilder, RecipeDescriptor recipeDescriptor)
            : base(recipeDescriptor)
        {
            FileBuilder = fileBuilder;
        }

        public IFileBuilder FileBuilder { get; }

        protected override Task WriteAsync()
        {
            // Add the recipe steps as its own file content
            return FileBuilder.SetFileAsync("Recipe.json", GetContent());
        }
    }
}
