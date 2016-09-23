namespace Orchard.Recipes.Models
{
    public class RecipeExecutionContext
    {
        public string ExecutionId { get; set; }
        public RecipeStepDescriptor RecipeStep { get; set; }
    }
}