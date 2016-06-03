using Newtonsoft.Json.Linq;

namespace Orchard.ContentManagement.Metadata.Records
{
    /// <summary>
    /// Represents a field and its settings in a part.
    /// </summary>
    public class ContentPartFieldDefinitionRecord
    {
        public ContentFieldDefinitionRecord ContentFieldDefinitionRecord { get; set; }

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the settings of the field for this part.
        /// </summary>
        public JObject Settings { get; set; }
    }
}