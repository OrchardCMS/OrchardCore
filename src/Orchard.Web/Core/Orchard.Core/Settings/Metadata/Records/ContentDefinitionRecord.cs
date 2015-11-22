using System.Collections.Generic;

namespace Orchard.Core.Settings.Metadata.Records
{
    public class ContentDefinitionRecord
    {
        public ContentDefinitionRecord()
        {
            ContentTypeDefinitionRecords = new List<ContentTypeDefinitionRecord>();
            ContentPartDefinitionRecords = new List<ContentPartDefinitionRecord>();
            ContentFieldDefinitionRecords = new List<ContentFieldDefinitionRecord>();
        }

        public int Id { get; set; }
        public IList<ContentTypeDefinitionRecord> ContentTypeDefinitionRecords { get; set; }
        public IList<ContentPartDefinitionRecord> ContentPartDefinitionRecords { get; set; }
        public IList<ContentFieldDefinitionRecord> ContentFieldDefinitionRecords { get; set; }
    }
}