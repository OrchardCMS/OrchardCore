using System.Xml.Linq;

namespace Orchard.Environment.Recipes.Services
{
    public class UpdateRecipeExecutionStepContext
    {
        public XDocument RecipeDocument { get; set; }
        public XElement Step { get; set; }
    }
}