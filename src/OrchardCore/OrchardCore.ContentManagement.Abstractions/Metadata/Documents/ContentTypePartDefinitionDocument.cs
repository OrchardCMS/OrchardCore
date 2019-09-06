using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Documents
{
    /// <summary>
    /// Represents a part and its settings in a type.
    /// </summary>
    public class ContentTypePartDefinitionDocument
    {
        /// <summary>
        /// Gets or sets the part name.
        /// </summary>
        public string PartName { get; set; }

        /// <summary>
        /// Gets or sets the name of the part.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the settings of the part for this type.
        /// </summary>
        public JObject Settings { get; set; }
    }
}