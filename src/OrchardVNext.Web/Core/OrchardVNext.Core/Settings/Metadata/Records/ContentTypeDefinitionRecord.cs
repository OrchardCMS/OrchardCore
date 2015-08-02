using System.Collections.Generic;
using OrchardVNext.ContentManagement;
using OrchardVNext.ContentManagement.Records;
using OrchardVNext.Data.Conventions;

namespace OrchardVNext.Core.Settings.Metadata.Records {
    public class ContentTypeDefinitionRecord : DocumentRecord {
        public ContentTypeDefinitionRecord() {
            ContentTypePartDefinitionRecords = new List<ContentTypePartDefinitionRecord>();
        }

        public string Name {
            get { return this.RetrieveValue(x => x.Name); }
            set { this.StoreValue(x => x.Name, value); }
        }
        public string DisplayName {
            get { return this.RetrieveValue(x => x.DisplayName); }
            set { this.StoreValue(x => x.DisplayName, value); }
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
        public virtual IList<ContentTypePartDefinitionRecord> ContentTypePartDefinitionRecords { get; set; }
    }

}
