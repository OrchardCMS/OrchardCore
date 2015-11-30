using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Orchard.ContentManagement.MetaData.Models
{
    public class ContentTypeDefinition
    {
        public ContentTypeDefinition(string name, string displayName, IEnumerable<ContentTypePartDefinition> parts, JObject settings)
        {
            Name = name;
            DisplayName = displayName;
            Parts = parts.ToList();
            Settings = settings;
        }

        public ContentTypeDefinition(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
            Parts = Enumerable.Empty<ContentTypePartDefinition>();
            Settings = new JObject();
        }

        [StringLength(128)]
        public string Name { get; private set; }
        [Required, StringLength(1024)]
        public string DisplayName { get; private set; }
        public IEnumerable<ContentTypePartDefinition> Parts { get; private set; }
        public JObject Settings { get; private set; }
    }
}