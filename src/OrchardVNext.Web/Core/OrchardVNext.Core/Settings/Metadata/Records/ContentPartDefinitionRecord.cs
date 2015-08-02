using System.Collections.Generic;
using OrchardVNext.ContentManagement;
using OrchardVNext.ContentManagement.Records;
using OrchardVNext.Data.Conventions;

namespace OrchardVNext.Core.Settings.Metadata.Records {
    public class ContentPartDefinitionRecord : DocumentRecord {
        public ContentPartDefinitionRecord() {
            ContentPartFieldDefinitionRecords = new List<ContentPartFieldDefinitionRecord>();
        }

        public string Name {
            get { return this.RetrieveValue(x => x.Name); }
            set { this.StoreValue(x => x.Name, value); }
        }
        public bool Hidden {
            get { return this.RetrieveValue(x => x.Hidden); }
            set { this.StoreValue(x => x.Hidden, value); }
        }
        [StringLengthMax]
        public string Settings {
            get { return this.RetrieveValue(x => x.Settings); }
            set { this.StoreValue(x => x.Settings, value); }
        }

        //[CascadeAllDeleteOrphan]
        public virtual IList<ContentPartFieldDefinitionRecord> ContentPartFieldDefinitionRecords { get; set; }

    }
}
