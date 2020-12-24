namespace OrchardCore.Lists.Models
{
    public enum ContentsStatus
    {
        Published,
        Draft,
        Owner
    }

    public class ContainedItemOptions
    {
        public string DisplayText { get; set; }
        public ContentsStatus Status { get; set; } = ContentsStatus.Published;
    }
}
