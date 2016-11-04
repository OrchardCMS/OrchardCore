using System.Collections.Generic;

namespace Orchard.Indexing.Settings
{
    /// <summary>
    /// Represents the indexing settings for a content type.
    /// </summary>
    public class TypeIndexSettings
    {
        /// <summary>
        /// The list of indexes that this type should be included into.
        /// </summary>
        public List<string> Indexes { get; set; } 
    }
}
