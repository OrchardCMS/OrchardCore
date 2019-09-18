namespace OrchardCore.Contents.Models
{
    public class IndexingPartSettings
    {
        //TODO don't index full content item #3809
        public bool IsNotIndexingFullTextOrAll { get; set; }

        public bool IndexBodyAspect { get; set; }

        public bool IndexDisplayText { get; set; }
    }
}