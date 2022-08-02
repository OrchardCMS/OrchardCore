using System;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Represents the indexing settings for a content part or a field.
    /// </summary>
    public class ContentIndexSettings
    {
        /// <summary>
        /// Set the content to be added in the index document. Will be tokenized or stored based on CLR Type.
        /// </summary>
        public bool Included { get; set; }

        /// <summary>
        /// Set the content to be stored in the index document
        /// </summary>
        public bool Stored { get; set; }

        /// <summary>
        /// Set the content to be analyzed/tokenized in the index document
        /// </summary>
        public bool Analyzed { get; set; }

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

