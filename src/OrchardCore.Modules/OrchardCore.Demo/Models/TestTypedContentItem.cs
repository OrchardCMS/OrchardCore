using OrchardCore.ContentManagement;

namespace OrchardCore.Demo.Models
{
    /// <summary>
    /// This can be extended to include content parts or fields.
    /// </summary>
    public class TestTypedContentItem : TypedContentItem
    {
        public TestTypedContentItem(ContentItem contentItem) : base(contentItem) { }
    }
}
