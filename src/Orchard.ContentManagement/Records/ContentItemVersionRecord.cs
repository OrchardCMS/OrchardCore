using System.Collections.Generic;

namespace Orchard.ContentManagement.Records {
    public class ContentItemVersionRecord {
        public int Id { get; set; }

        public ContentItemVersionRecord()
        {
            Parts = new List<ContentPart>();
        }
        public int ContentItemRecordId { get; set; }
        public string ContentType { get; set; }
        public int Number { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }
        public IList<ContentPart> Parts { get; set; }
    }
}