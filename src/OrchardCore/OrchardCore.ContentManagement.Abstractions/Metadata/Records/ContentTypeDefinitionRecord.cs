using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentTypeDefinitionRecord
    {
        public ContentTypeDefinitionRecord()
        {
            ContentTypePartDefinitionRecords = ImmutableArray.Create<ContentTypePartDefinitionRecord>();
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public JObject Settings { get; set; }

        public ImmutableArray<ContentTypePartDefinitionRecord> ContentTypePartDefinitionRecords { get; set; }

        public ContentTypeDefinitionRecord Clone()
        {
            return new ContentTypeDefinitionRecord()
            {
                Name = Name,
                DisplayName = DisplayName,
                Settings = new JObject(Settings),
                ContentTypePartDefinitionRecords = ContentTypePartDefinitionRecords
            };
        }
    }
}
