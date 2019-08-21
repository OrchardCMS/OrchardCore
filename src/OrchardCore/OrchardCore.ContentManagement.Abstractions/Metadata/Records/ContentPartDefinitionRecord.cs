using System.Collections.Immutable;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentPartDefinitionRecord
    {
        public ContentPartDefinitionRecord()
        {
            ContentPartFieldDefinitionRecords = ImmutableArray.Create<ContentPartFieldDefinitionRecord>();
            Settings = new JObject();
        }

        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the settings of a part, like description, or any property that a module would attach
        /// to a part.
        /// </summary>
        public JObject Settings { get; set; }

        public ImmutableArray<ContentPartFieldDefinitionRecord> ContentPartFieldDefinitionRecords { get; set; }

        public ContentPartDefinitionRecord Clone()
        {
            return new ContentPartDefinitionRecord()
            {
                Name = Name,
                Settings = new JObject(Settings),
                ContentPartFieldDefinitionRecords = ContentPartFieldDefinitionRecords
            };
        }
    }
}
