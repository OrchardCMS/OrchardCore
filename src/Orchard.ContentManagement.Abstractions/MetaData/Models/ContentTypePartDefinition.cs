using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement.MetaData.Models
{
    public class ContentTypePartDefinition
    {
        public ContentTypePartDefinition(string name, ContentPartDefinition contentPartDefinition, JObject settings)
        {
            Name = name;
            PartDefinition = contentPartDefinition;
            Settings = settings;
        }

        public string Name { get; private set; }
        public ContentPartDefinition PartDefinition { get; private set; }
        public JObject Settings { get; private set; }
        public ContentTypeDefinition ContentTypeDefinition { get; set; }
    }
}