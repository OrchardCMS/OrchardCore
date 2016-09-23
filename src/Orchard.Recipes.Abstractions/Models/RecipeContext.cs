namespace Orchard.Recipes.Models
{
    public class RecipeContext
    {
        public RecipeStepDescriptor RecipeStep { get; set; }
        public bool Executed { get; set; }
        public string ExecutionId { get; set; }
    }
}