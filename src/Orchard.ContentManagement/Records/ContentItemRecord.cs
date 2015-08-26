using System.Collections.Generic;

namespace Orchard.ContentManagement.Records {
    public class ContentItemRecord : DocumentRecord {
        public ContentItemRecord() {
            // ReSharper disable DoNotCallOverridableMethodsInConstructor
            Versions = new List<ContentItemVersionRecord>();
            // ReSharper restore DoNotCallOverridableMethodsInConstructor
        }
        
        public virtual ContentTypeRecord ContentType { get; set; }
        public virtual IList<ContentItemVersionRecord> Versions { get; set; }
    }
}