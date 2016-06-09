using System.Xml.Linq;

namespace Orchard.Recipes.Models
{
    public class ConfigurationContext
    {
        protected ConfigurationContext(XElement configurationElement)
        {
            ConfigurationElement = configurationElement;
        }

        public XElement ConfigurationElement { get; set; }
    }
}