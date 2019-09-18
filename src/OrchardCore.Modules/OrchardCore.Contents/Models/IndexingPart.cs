using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Models
{
    /// <summary>
    /// When attached to a content type, provides a way to define if
    /// the content item will be indexed in the full-text index.
    /// </summary>
    public class IndexingPart : ContentPart
    {
        public bool IsIndexed { get; set; } = true;
    }
}
