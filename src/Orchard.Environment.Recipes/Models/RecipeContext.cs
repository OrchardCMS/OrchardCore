namespace Orchard.Environment.Recipes.Models
{
    public class RecipeContext
    {
        public RecipeStep RecipeStep { get; set; }
        public bool Executed { get; set; }
        public string ExecutionId { get; set; }
    }
}