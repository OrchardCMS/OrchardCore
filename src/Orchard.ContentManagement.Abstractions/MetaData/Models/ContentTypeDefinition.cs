using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Orchard.ContentManagement.Metadata.Models
{
    public class ContentTypeDefinition
    {
        public ContentTypeDefinition(string name, string displayName, IEnumerable<ContentTypePartDefinition> parts, JObject settings)
        {
            Name = name;
            DisplayName = displayName;
            Parts = parts.ToList();
            Settings = settings;

            foreach(var part in Parts)
            {
                part.ContentTypeDefinition = this;
            }
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

        /// <summary>
        /// Returns the <see cref="DisplayName"/> value of the type if defined,
        /// or the <see cref="Name"/> otherwise.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(DisplayName)
                ? Name
                : DisplayName;
        }
    }

}