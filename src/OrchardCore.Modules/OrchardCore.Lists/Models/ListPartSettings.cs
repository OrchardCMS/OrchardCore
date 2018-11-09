namespace OrchardCore.Lists.Models
{
    public class ListPartSettings
    {
        public int PageSize { get; set; } = 10;
        public bool PagerSlimDisabled { get; set; }
        public string[] ContainedContentTypes { get; set; }
    }
}