using System.Xml.Linq;

namespace Orchard.Environment.Recipes.Models
{
    public class RecipeBuilderStepConfigurationContext : ConfigurationContext
    {
        public RecipeBuilderStepConfigurationContext(XElement configurationElement) : base(configurationElement)
        {
        }
    }
}