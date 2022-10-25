namespace OrchardCore.Lists.Models
{
    public class ListPartSettings
    {
        public int PageSize { get; set; } = 10;
        public string[] ContainedContentTypes { get; set; }
        public bool EnableOrdering { get; set; }
        public bool ShowHeader { get; set; }
    }
}
