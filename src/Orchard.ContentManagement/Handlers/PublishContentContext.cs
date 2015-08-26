using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {
    public class PublishContentContext : ContentContextBase {
        public PublishContentContext(ContentItem contentItem, ContentItemVersionRecord previousItemVersionRecord) : base(contentItem) {
            PublishingItemVersionRecord = contentItem.VersionRecord;
            PreviousItemVersionRecord = previousItemVersionRecord;
        }

        public ContentItemVersionRecord PublishingItemVersionRecord { get; set; }
        public ContentItemVersionRecord PreviousItemVersionRecord { get; set; }

        public bool Cancel { get; set; }
    }
}