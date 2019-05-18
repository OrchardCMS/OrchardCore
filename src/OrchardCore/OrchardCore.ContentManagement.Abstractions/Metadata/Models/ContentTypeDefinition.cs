using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json.Linq;
using OrchardCore.Localization;

namespace OrchardCore.ContentManagement.Metadata.Models
{
    public class ContentTypeDefinition : ContentDefinition
    {
        public ContentTypeDefinition(string name, LocalizedObject displayName, IEnumerable<ContentTypePartDefinition> parts, JObject settings)
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

        public ContentTypeDefinition(string name, LocalizedObject displayName)
        {
            Name = name;
            DisplayName = displayName;
            Parts = Enumerable.Empty<ContentTypePartDefinition>();
            Settings = new JObject();
        }

        [Required, StringLength(1024)]
        public LocalizedObject DisplayName { get; private set; }

        public IEnumerable<ContentTypePartDefinition> Parts { get; private set; }

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