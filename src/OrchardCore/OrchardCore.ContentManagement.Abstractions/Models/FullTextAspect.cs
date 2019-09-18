namespace OrchardCore.ContentManagement.Models
{
    public class FullTextAspect
    {
        //TODO add a part that would use this to prevent indexing a specific content item
        public bool Indexed { get; set; } = true;
        public string FullText { get; set; }
    }
}
