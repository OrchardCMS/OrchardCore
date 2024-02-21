using System.Text;
using System.Text.Json.Nodes;
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

        public override async Task FinalizeAsync()
        {
            Recipe["steps"] = JArray.FromObject(Steps);

            // Add the recipe steps as its own file content
            await FileBuilder.SetFileAsync("Recipe.json", GetContent());
        }

        protected byte[] GetContent()
        {
            return Encoding.UTF8.GetBytes(Recipe.ToString());
        }
    }
}
