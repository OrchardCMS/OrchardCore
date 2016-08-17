using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement.Metadata.Models
{
    public class ContentPartFieldDefinition
    {
        public ContentPartFieldDefinition(ContentFieldDefinition contentFieldDefinition, string name, JObject settings)
        {
            Name = name;
            FieldDefinition = contentFieldDefinition;
            Settings = settings;
        }

        public string Name { get; private set; }
        public ContentFieldDefinition FieldDefinition { get; private set; }
        public JObject Settings { get; private set; }
        public ContentPartDefinition PartDefinition { get; set; }
    }
}