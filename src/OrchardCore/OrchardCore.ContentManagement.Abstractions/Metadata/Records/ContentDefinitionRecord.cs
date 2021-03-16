using System.Collections.Generic;
using OrchardCore.Data.Documents;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentDefinitionRecord : Document
    {
        public ContentDefinitionRecord()
        {
            ContentTypeDefinitionRecords = new List<ContentTypeDefinitionRecord>();
            ContentPartDefinitionRecords = new List<ContentPartDefinitionRecord>();
        }

        public IList<ContentTypeDefinitionRecord> ContentTypeDefinitionRecords { get; set; }
        public IList<ContentPartDefinitionRecord> ContentPartDefinitionRecords { get; set; }
    }
}
