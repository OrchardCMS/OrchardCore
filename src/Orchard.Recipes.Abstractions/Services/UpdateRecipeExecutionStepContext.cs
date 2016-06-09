using System.Xml.Linq;

namespace Orchard.Recipes.Services
{
    public class UpdateRecipeExecutionStepContext
    {
        public XDocument RecipeDocument { get; set; }
        public XElement Step { get; set; }
    }
}