using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Models
{
    public class ConfigurationContext
    {
        protected ConfigurationContext(JsonObject configurationElement)
        {
            ConfigurationElement = configurationElement;
        }

        public JsonObject ConfigurationElement { get; set; }
    }
}
