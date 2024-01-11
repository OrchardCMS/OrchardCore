using System;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Represents the indexing settings for a content part or a field.
    /// </summary>
    [Obsolete("This class has been deprecated and we will be removed in the next major release, please use IContentIndexSettings instead.", false)]
    public class ContentIndexSettings
    {
        /// <summary>
        /// Set the content to be added in the index document. Will be indexed based on CLR Type.
        /// </summary>
        public bool Included { get; set; }

        /// <summary>
        /// Set the content to be indexed in the index as a keyword (tokenized as a single term).
        /// See Lucene StringField.
        /// </summary>
        public bool Keyword { get; set; }

        /// <summary>
        /// Set the content to be stored in the index document. The original value will be stored.
        /// </summary>
        public bool Stored { get; set; }

        public DocumentIndexOptions ToOptions()
        {
            var options = DocumentIndexOptions.None;

            if (Stored)
            {
                options |= DocumentIndexOptions.Store;
            }

            if (Keyword)
            {
                options |= DocumentIndexOptions.Keyword;
            }

            return options;
        }
    }
}

