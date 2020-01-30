using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public class ContentPartDefinition : ContentDefinition
    {
        public ContentPartDefinition(string name)
        {
            Name = name;
            Fields = Enumerable.Empty<ContentPartFieldDefinition>();
            Settings = new JObject();
        }

        public ContentPartDefinition(string name, IEnumerable<ContentPartFieldDefinition> fields, JObject settings)
        {
            Name = name;
            Fields = fields.ToList();
            Settings = new JObject(settings);

            foreach (var field in Fields)
            {
                field.PartDefinition = this;
            }
        }

        public IEnumerable<ContentPartFieldDefinition> Fields { get; private set; }
    }
}
