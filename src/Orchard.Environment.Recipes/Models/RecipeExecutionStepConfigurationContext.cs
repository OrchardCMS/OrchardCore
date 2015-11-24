using System.Xml.Linq;

namespace Orchard.Environment.Recipes.Models
{
    public class RecipeExecutionStepConfigurationContext : ConfigurationContext
    {
        public RecipeExecutionStepConfigurationContext(XElement configurationElement) : base(configurationElement)
        {
        }
    }
}