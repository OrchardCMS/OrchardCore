namespace OrchardCore.Lists.ViewModels
{
    public class ListPartFilterViewModel
    {
        public string DisplayText { get; set; }

        public ContentsStatus Status { get; set; }

        public enum ContentsStatus
        {
            None,
            Draft,
            Published,
            Owner
        }
    }
}
