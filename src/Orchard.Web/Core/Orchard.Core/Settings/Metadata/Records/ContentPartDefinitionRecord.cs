using System.Collections.Generic;
using Orchard.Data.Conventions;

namespace Orchard.Core.Settings.Metadata.Records
{
    public class ContentPartDefinitionRecord
    {
        public ContentPartDefinitionRecord()
        {
            ContentPartFieldDefinitionRecords = new List<ContentPartFieldDefinitionRecord>();
        }

        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public virtual bool Hidden { get; set; }
        [StringLengthMax]
        public virtual string Settings { get; set; }

        public virtual IList<ContentPartFieldDefinitionRecord> ContentPartFieldDefinitionRecords { get; set; }
    }
}