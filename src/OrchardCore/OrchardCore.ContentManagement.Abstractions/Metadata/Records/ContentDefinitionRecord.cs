using System.Collections.Immutable;
using System.Linq;

namespace OrchardCore.ContentManagement.Metadata.Records
{
    public class ContentDefinitionRecord
    {
        public ContentDefinitionRecord()
        {
            ContentTypeDefinitionRecords = ImmutableArray.Create<ContentTypeDefinitionRecord>();
            ContentPartDefinitionRecords = ImmutableArray.Create<ContentPartDefinitionRecord>();
        }

        public int Id { get; set; }
        public ImmutableArray<ContentTypeDefinitionRecord> ContentTypeDefinitionRecords { get; set; }
        public ImmutableArray<ContentPartDefinitionRecord> ContentPartDefinitionRecords { get; set; }
        public int Serial { get; set; }

        public ContentDefinitionRecord Clone()
        {
            return new ContentDefinitionRecord()
            {
                ContentTypeDefinitionRecords = ContentTypeDefinitionRecords
                    .Select(type => type.Clone())
                    .ToImmutableArray(),

                ContentPartDefinitionRecords = ContentPartDefinitionRecords
                    .Select(part => part.Clone())
                    .ToImmutableArray(),

                Serial = Serial
            };
        }
    }
}
