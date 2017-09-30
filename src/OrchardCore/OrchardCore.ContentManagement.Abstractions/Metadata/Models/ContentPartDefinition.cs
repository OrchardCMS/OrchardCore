using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public class ContentPartDefinition : ContentDefinition
    {
        public ContentPartDefinition(string name)
        {
            Name = name;
            DisplayName = name.TrimEnd("Part").CamelFriendly();
            Fields = Enumerable.Empty<ContentPartFieldDefinition>();
            Settings = new JObject();
        }

        public ContentPartDefinition(string name, string displayName)
        {
            Name = name;
            DisplayName = displayName;
            Fields = Enumerable.Empty<ContentPartFieldDefinition>();
            Settings = new JObject();
        }

        public ContentPartDefinition(string name, string displayName, IEnumerable<ContentPartFieldDefinition> fields, JObject settings)
        {
            Name = name;
            DisplayName = displayName;
            Fields = fields.ToList();
            Settings = settings;

            foreach (var field in Fields)
            {
                field.PartDefinition = this;
            }
        }

        public ContentPartDefinition(string name, IEnumerable<ContentPartFieldDefinition> fields, JObject settings)
        {
            Name = name;
            DisplayName = name.TrimEnd("Part").CamelFriendly();
            Fields = fields.ToList();
            Settings = settings;

            foreach (var field in Fields)
            {
                field.PartDefinition = this;
            }
        }

        [Required, StringLength(1024)]
        public string DisplayName { get; private set; }
        public IEnumerable<ContentPartFieldDefinition> Fields { get; private set; }

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