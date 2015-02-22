using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.ContentManagement.Handlers {
    public class UpdateContentContext : ContentContextBase {
        public UpdateContentContext(ContentItem contentItem) : base(contentItem) {
            UpdatingItemVersionRecord = contentItem.VersionRecord;
        }

        public ContentItemVersionRecord UpdatingItemVersionRecord { get; set; }
    }
}