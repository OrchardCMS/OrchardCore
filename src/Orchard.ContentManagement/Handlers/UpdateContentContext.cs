using Orchard.ContentManagement.Records;

namespace Orchard.ContentManagement.Handlers {
    public class UpdateContentContext : ContentContextBase {
        public UpdateContentContext(ContentItem contentItem) : base(contentItem) {
            UpdatingItemVersionRecord = contentItem.VersionRecord;
        }

        public ContentItemVersionRecord UpdatingItemVersionRecord { get; set; }
    }
}