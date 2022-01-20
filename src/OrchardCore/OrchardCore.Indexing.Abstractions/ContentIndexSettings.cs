using System.ComponentModel;
using Newtonsoft.Json;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Represents the indexing settings for a content part or a field.
    /// </summary>
    public class ContentIndexSettings
    {
        public bool Included { get; set; }
        public bool Stored { get; set; }
        public bool Analyzed { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
        public bool SQLIncluded { get; set; } = true;

        public DocumentIndexOptions ToOptions()
        {
            var options = DocumentIndexOptions.None;

            if (Stored)
            {
                options |= DocumentIndexOptions.Store;
            }

            if (Analyzed)
            {
                options |= DocumentIndexOptions.Analyze;
            }

            return options;
        }
    }
}
