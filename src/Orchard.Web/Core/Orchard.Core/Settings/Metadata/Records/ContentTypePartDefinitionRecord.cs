using Newtonsoft.Json.Linq;

namespace Orchard.Core.Settings.Metadata.Records
{
    /// <summary>
    /// Represents a part and its settings in a type.
    /// </summary>
    public class ContentTypePartDefinitionRecord
    {
        public ContentTypePartDefinitionRecord(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the name of the part.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the settings of the part for this type.
        /// </summary>
        public JObject Settings { get; set; }
    }
}