using OrchardVNext.ContentManagement.Records;

namespace OrchardVNext.ContentManagement.Handlers {
    public class CreateContentContext : ContentContextBase {
        public CreateContentContext(ContentItem contentItem) : base(contentItem) {
            ContentItemVersionRecord = contentItem.VersionRecord;
        }

        public ContentItemVersionRecord ContentItemVersionRecord { get; set; }
    }
}
