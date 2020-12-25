namespace OrchardCore.Lists.Models
{
    public enum ContentsStatus
    {
        Published,
        Latest,
        Draft,
        Owner
    }

    public class ContainedItemOptions
    {
        public string DisplayText { get; set; }
        public ContentsStatus Status { get; set; } = ContentsStatus.Published;
    }
}
