namespace OrchardCore.ContentManagement.Metadata.Settings
{
    public class ContentTypeIndexingSettings
    {
        /// <summary>
        /// Used to determine if this content type supports FullText indexing
        /// </summary>
        public bool IsFullText { get; set; }
        /// <summary>
        /// Used to define the string values that should be indexed for a FullText search
        /// </summary>
        public string FullText { get; set; }
    }
}
