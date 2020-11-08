namespace OrchardCore.Lists.Models
{
    public enum ContentsStatus
    {
        None,
        Draft,
        Published,
        Owner
    }

    public class ContainedItemOptions
    {
        public string DisplayText { get; set; }
        public ContentsStatus Status { get; set; }
    }
}
