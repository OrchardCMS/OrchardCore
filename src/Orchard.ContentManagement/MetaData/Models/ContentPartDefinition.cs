using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement.MetaData.Models
{
    public class ContentPartDefinition
    {
        public ContentPartDefinition(string name, IEnumerable<ContentPartFieldDefinition> fields, JObject settings)
        {
            Name = name;
            Fields = fields.ToList();
            Settings = settings;
        }

        public ContentPartDefinition(string name)
        {
            Name = name;
            Fields = Enumerable.Empty<ContentPartFieldDefinition>();
            Settings = new JObject();
        }

        public string Name { get; private set; }
        public IEnumerable<ContentPartFieldDefinition> Fields { get; private set; }
        public JObject Settings { get; set; }
    }
}