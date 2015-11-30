using System.Collections.Generic;
using Orchard.Data.Conventions;
using Newtonsoft.Json.Linq;

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
        public virtual JObject Settings { get; set; }

        public virtual IList<ContentPartFieldDefinitionRecord> ContentPartFieldDefinitionRecords { get; set; }
    }
}