using System.ComponentModel;

namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public class ContentTypeIndexingSettings
    {
        /// <summary>
        /// Used to determine if this content type supports full-text indexing
        /// </summary>
        public bool IsFullText { get; set; }
        /// <summary>
        /// Used to define the string values that should be indexed for a full-text search
        /// </summary>
        public string FullText { get; set; }

        [DefaultValue(true)]
        public bool IndexBodyAspect { get; set; } = true;

        [DefaultValue(true)]
        public bool IndexDisplayText { get; set; } = true;
    }
}
