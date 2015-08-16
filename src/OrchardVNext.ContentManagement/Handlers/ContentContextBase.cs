using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.ContentManagement.Handlers {
    public class ContentContextBase {
        protected ContentContextBase (ContentItem contentItem) {
            ContentItem = contentItem;
            Id = contentItem.Id;
            ContentType = contentItem.ContentType;
            ContentItemRecord = contentItem.Record;
        }

        public int Id { get; private set; }
        public string ContentType { get; private set; }
        public ContentItem ContentItem { get; private set; }
        public ContentItemRecord ContentItemRecord { get; private set; }
    }
}