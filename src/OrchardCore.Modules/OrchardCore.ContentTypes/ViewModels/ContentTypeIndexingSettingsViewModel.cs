namespace OrchardCore.ContentTypes.ViewModels
{
    public class ContentTypeIndexingSettingsViewModel
    {
        public bool IsFullTextLiquid { get; set; }
        public string FullTextLiquid { get; set; }
        public bool IndexBodyAspect { get; set; }
        public bool IndexDisplayText { get; set; }
    }
}
