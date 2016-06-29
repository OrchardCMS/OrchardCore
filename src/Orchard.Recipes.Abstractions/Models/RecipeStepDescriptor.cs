using Newtonsoft.Json.Linq;

namespace Orchard.Recipes.Models
{
    public class RecipeStepDescriptor
    {
        public string Name { get; set; }
        public JObject Step { get; private set; }
    }
}