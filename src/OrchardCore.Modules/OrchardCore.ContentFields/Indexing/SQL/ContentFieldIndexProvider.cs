using OrchardCore.ContentManagement;
using OrchardCore.Data;
using YesSql.Indexes;

namespace OrchardCore.ContentFields.Indexing.SQL
{
    public abstract class ContentFieldIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string ContentItemVersionId { get; set; }
        public string ContentType { get; set; }
        public string ContentPart { get; set; }
        public string ContentField { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }
    }

    public abstract class ContentFieldIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
    {
    }
}
