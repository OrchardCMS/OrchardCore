using OrchardVNext.Data;

namespace OrchardVNext.ContentManagement.Records {
    [Persistent]
    public class ContentItemVersionRecord : DocumentRecord {
        public ContentItemRecord ContentItemRecord { get; set; }
        public int Number { get; set; }

        public bool Published { get; set; }
        public bool Latest { get; set; }
    }
}