using Newtonsoft.Json.Linq;

namespace Orchard.Recipes.Models
{
    public class RecipeExecutionContext
    {
        public string ExecutionId { get; set; }
        public object Environment { get; set; }
        public string Name { get; set; }
        public JObject Step { get; set; }
    }
}