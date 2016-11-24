namespace Orchard.Indexing
{
    /// <summary>
    /// Represents the indexing settings for a content part or a field.
    /// </summary>
    public class ContentIndexSettings
    {
        public bool Included { get; set; }
        public bool Stored { get; set; }
        public bool Analyzed { get; set; }
        public bool Sanitized { get; set; }
        public bool Tokenized { get; set; }
        public string Template { get; set; }

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

            if (Sanitized)
            {
                options |= DocumentIndexOptions.Sanitize;
            }

            return options;
        }
    }
}
