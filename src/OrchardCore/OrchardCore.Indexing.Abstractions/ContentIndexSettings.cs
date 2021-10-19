using System;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Represents the indexing settings for a content part or a field.
    /// </summary>
    [Obsolete("This class has been deprecated and we will be removed in the next major release, please use IContentIndexSettings instead.", false)]
    public class ContentIndexSettings
    {
        public bool Included { get; set; }
        public bool Stored { get; set; }
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
