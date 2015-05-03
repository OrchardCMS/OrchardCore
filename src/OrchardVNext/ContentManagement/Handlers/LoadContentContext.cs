using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.ContentManagement.Handlers {
    public class LoadContentContext : ContentContextBase {
        public LoadContentContext(ContentItem contentItem) : base(contentItem) {
            ContentItemVersionRecord = contentItem.VersionRecord;
        }

        public ContentItemVersionRecord ContentItemVersionRecord { get; set; }
    }
}