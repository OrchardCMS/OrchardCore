namespace OrchardCore.Contents.ViewModels
{
    public class IndexingPartSettingsViewModel
    {
        //Toggle between not indexing the entire content item or not indexing full-text index only
        public bool IsNotIndexingFullTextOrAll { get; set; }

        public bool IndexBodyAspect { get; set; }

        public bool IndexDisplayText { get; set; }
    }
}
