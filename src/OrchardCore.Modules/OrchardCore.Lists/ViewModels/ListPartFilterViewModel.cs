namespace OrchardCore.Lists.ViewModels
{
    public class ListPartFilterViewModel
    {
        public string DisplayText { get; set; }

        public string SelectedContentType { get; set; }

        public bool CanCreateSelectedContentType { get; set; }

        public ContentsOrder OrderBy { get; set; }

        public ContentsStatus Status { get; set; }

        public ContentsBulkAction BulkAction { get; set; }

        public enum ContentsOrder
        {
            Modified,
            Published,
            Created,
            Title,
        }

        public enum ContentsStatus
        {
            None,
            Draft,
            Published,
            AllVersions,
            Latest,
            Owner
        }

        public enum ContentsBulkAction
        {
            None,
            PublishNow,
            Unpublish,
            Remove
        }
    }
}
