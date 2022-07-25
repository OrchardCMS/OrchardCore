using System;

namespace OrchardCore.Indexing
{
    /// <summary>
    /// Represents the indexing settings for a content part or a field.
    /// </summary>
    public class ContentIndexSettings
    {
        public bool Included { get; set; }

        public DocumentIndexOptions ToOptions()
        {
            var options = DocumentIndexOptions.None;

            return options;
        }
    }
}
