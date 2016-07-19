using Newtonsoft.Json.Linq;

namespace Orchard.Recipes.Models
{
    public class RecipeStepDescriptor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RecipeName { get; set; }
        public JToken Step { get; set; }
    }
}