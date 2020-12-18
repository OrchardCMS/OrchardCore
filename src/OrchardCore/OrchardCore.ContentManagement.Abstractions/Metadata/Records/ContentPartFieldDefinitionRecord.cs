using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    /// <summary>
    /// Represents a field and its settings in a part.
    /// </summary>
    public class ContentPartFieldDefinitionRecord
    {
        /// <summary>
        /// Gets or sets the field name, e.g. BooleanField.
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Gets or sets the name of the field, e.g. Age.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the settings of the field for this part.
        /// </summary>
        public JObject Settings { get; set; }
    }
}
