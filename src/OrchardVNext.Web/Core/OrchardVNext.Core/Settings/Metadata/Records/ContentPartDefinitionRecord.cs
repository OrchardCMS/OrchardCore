using System.Collections.Generic;
using OrchardVNext.Data;
using OrchardVNext.Data.Conventions;

namespace OrchardVNext.Core.Settings.Metadata.Records {
    [Persistent]
    public class ContentPartDefinitionRecord {
        public ContentPartDefinitionRecord() {
            ContentPartFieldDefinitionRecords = new List<ContentPartFieldDefinitionRecord>();
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual bool Hidden { get; set; }
        [StringLengthMax]
        public virtual string Settings { get; set; }

        //[CascadeAllDeleteOrphan]
        public virtual IList<ContentPartFieldDefinitionRecord> ContentPartFieldDefinitionRecords { get; set; }

    }
}
