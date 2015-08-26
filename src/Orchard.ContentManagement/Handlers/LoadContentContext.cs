using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {
    public class LoadContentContext : ContentContextBase {
        public LoadContentContext(ContentItem contentItem) : base(contentItem) {
            ContentItemVersionRecord = contentItem.VersionRecord;
        }

        public ContentItemVersionRecord ContentItemVersionRecord { get; set; }
    }
}