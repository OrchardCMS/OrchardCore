namespace OrchardCore.ContentManagement.Models
{
    public class FullTextAspect
    {
        public bool Indexed { get; set; } = true;
        public string FullText { get; set; }
    }
}
