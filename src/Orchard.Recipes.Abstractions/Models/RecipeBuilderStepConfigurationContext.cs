using System.Xml.Linq;

namespace Orchard.Recipes.Models
{
    public class RecipeBuilderStepConfigurationContext : ConfigurationContext
    {
        public RecipeBuilderStepConfigurationContext(XElement configurationElement) : base(configurationElement)
        {
        }
    }
}