using YesSql.Core.Indexes;

namespace Orchard.ContentManagement.Records
{
    public class ContentItemVersionRecordIndex : MapIndex
    {
        public int ContentItemRecordId { get; set; }
        public int Number { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }
        public string ContentType { get; set; }
    }

    public class ContentItemVersionRecordIndexProvider : IndexProvider<ContentItemVersionRecord>
    {
        public override void Describe(DescribeContext<ContentItemVersionRecord> context)
        {
            context.For<ContentItemVersionRecordIndex>()
                .Map(civr => new ContentItemVersionRecordIndex
                {
                    ContentItemRecordId = civr.ContentItemRecordId,
                    Latest = civr.Latest,
                    Number = civr.Number,
                    Published = civr.Published,
                    ContentType = civr.ContentType
                });
        }
    }
}
