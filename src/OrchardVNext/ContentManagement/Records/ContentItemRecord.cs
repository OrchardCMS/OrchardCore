using System.Collections.Generic;
using OrchardVNext.Data;

namespace OrchardVNext.ContentManagement.Records {
    [Persistent]
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