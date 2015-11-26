using Newtonsoft.Json.Linq;
using Orchard.Utility;

namespace Orchard.ContentManagement.MetaData.Models
{
    public class ContentPartFieldDefinition
    {
        public ContentPartFieldDefinition(string name)
        {
            Name = name;
            DisplayName = Name.CamelFriendly();
            FieldDefinition = new ContentFieldDefinition(null);
            Settings = new JObject();
        }
        public ContentPartFieldDefinition(ContentFieldDefinition contentFieldDefinition, string name, JObject settings)
        {
            Name = name;
            DisplayName = Name.CamelFriendly();
            FieldDefinition = contentFieldDefinition;
            Settings = settings;
        }

        public string Name { get; private set; }

        public string DisplayName { get; set; }

        public ContentFieldDefinition FieldDefinition { get; private set; }
        public JObject Settings { get; private set; }
    }
}