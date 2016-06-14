using Newtonsoft.Json.Linq;

namespace Orchard.Recipes.Models
{
    public class RecipeBuilderStepConfigurationContext : ConfigurationContext
    {
        public RecipeBuilderStepConfigurationContext(JObject configurationElement) : base(configurationElement)
        {
        }
    }
}