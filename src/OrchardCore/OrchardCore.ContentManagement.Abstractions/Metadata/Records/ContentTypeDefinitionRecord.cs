using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentTypeDefinitionRecord : INamedContentDefinitionRecord
    {
        public ContentTypeDefinitionRecord()
        {
            ContentTypePartDefinitionRecords = [];
        }

        public string Name { get; set; }

        public string DisplayName { get; set; }

        public JObject Settings { get; set; }

        public IList<ContentTypePartDefinitionRecord> ContentTypePartDefinitionRecords { get; set; }
    }
}
