using System.Collections.Generic;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentDefinitionRecord
    {
        public ContentDefinitionRecord()
        {
            ContentTypeDefinitionRecords = new List<ContentTypeDefinitionRecord>();
            ContentPartDefinitionRecords = new List<ContentPartDefinitionRecord>();
        }

        public IList<ContentTypeDefinitionRecord> ContentTypeDefinitionRecords { get; set; }
        public IList<ContentPartDefinitionRecord> ContentPartDefinitionRecords { get; set; }
        public int Serial { get; set; }
    }
}
