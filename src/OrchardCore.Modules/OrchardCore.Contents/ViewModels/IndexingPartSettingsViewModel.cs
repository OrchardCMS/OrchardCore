namespace OrchardCore.Contents.ViewModels
{
    public class IndexingPartSettingsViewModel
    {
        //Toggle between not indexing the entire content item or not indexing only full-text index
        public bool IsNotIndexingFullTextOrAll { get; set; }

        public bool IndexBodyAspect { get; set; } = true;

        public bool IndexDisplayText { get; set; } = true;
    }
}
