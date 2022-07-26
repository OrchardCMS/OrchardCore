using System;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Represents the indexing settings for a content part or a field.
    /// </summary>
    public class ContentIndexSettings
    {
        /// <summary>
        /// Set the content to be tokenized in the index document
        /// </summary>
        public bool Analyzed { get; set; }

        /// <summary>
        /// Set the content to be stored in the index document
        /// </summary>
        public bool Stored { get; set; }

        public DocumentIndexOptions ToOptions()
        {
            var options = DocumentIndexOptions.None;

            if (Analyzed)
            {
                options |= DocumentIndexOptions.Analyze;
            }

            if (Stored)
            {
                options |= DocumentIndexOptions.Store;
            }

            return options;
        }
    }
}
