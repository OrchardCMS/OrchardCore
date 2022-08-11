using System;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Represents the indexing settings for a content part or a field.
    /// </summary>
    public class ContentIndexSettings
    {
        /// <summary>
        /// Set the content to be added in the index document. Will be indexed based on CLR Type.
        /// </summary>
        public bool Included { get; set; }

        /// <summary>
        /// Set the content to be stored in the index document
        /// </summary>
        public bool Stored { get; set; }

        public DocumentIndexOptions ToOptions()
        {
            var options = DocumentIndexOptions.None;

            if (Stored)
            {
                options |= DocumentIndexOptions.Store;
            }

            return options;
        }
    }
}

