namespace OrchardCore.Taxonomies.ViewModels
{
    public class TaxonomyContentsAdminListSettingsViewModel
    {
        public TaxonomyEntry[] TaxonomyEntries { get; set; }
    }

    public class TaxonomyEntry
    {
        public string DisplayText { get; set; }
        public string ContentItemId { get; set; }
        public bool IsChecked { get; set; }
    }
}
