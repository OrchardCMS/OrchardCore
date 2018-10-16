using System.Collections.Generic;
using Newtonsoft.Json;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentDefinitionRecord
    {
        public ContentDefinitionRecord()
        {
            ContentTypeDefinitionRecords = new List<ContentTypeDefinitionRecord>();
            ContentPartDefinitionRecords = new List<ContentPartDefinitionRecord>();
        }

        public int Id { get; set; }
        public IList<ContentTypeDefinitionRecord> ContentTypeDefinitionRecords { get; set; }
        public IList<ContentPartDefinitionRecord> ContentPartDefinitionRecords { get; set; }
        public int Serial { get; set; }
    }
}