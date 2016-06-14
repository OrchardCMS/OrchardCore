using Newtonsoft.Json.Linq;

namespace Orchard.Recipes.Models
{
    public class ConfigurationContext
    {
        protected ConfigurationContext(JObject configurationElement)
        {
            ConfigurationElement = configurationElement;
        }

        public JObject ConfigurationElement { get; set; }
    }
}