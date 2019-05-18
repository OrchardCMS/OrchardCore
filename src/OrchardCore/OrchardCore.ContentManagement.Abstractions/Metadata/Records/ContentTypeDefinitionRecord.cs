using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using OrchardCore.Localization;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentTypeDefinitionRecord
    {
        public ContentTypeDefinitionRecord()
        {
            ContentTypePartDefinitionRecords = new List<ContentTypePartDefinitionRecord>();
        }

        public string Name { get; set; }

        public LocalizedObject DisplayName { get; set; }

        public JObject Settings { get; set; }

        public IList<ContentTypePartDefinitionRecord> ContentTypePartDefinitionRecords { get; set; }
    }
}