using OrchardCore.Contents.AuditTrail.Indexes;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.Contents.AuditTrail.Migrations
{
    public class ContentAuditTrailMigrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<ContentAuditTrailEventIndex>(table => table
                .Column<string>(nameof(ContentAuditTrailEventIndex.ContentItemId), column => column.WithLength(26))
                .Column<string>(nameof(ContentAuditTrailEventIndex.ContentType))
                .Column<string>(nameof(ContentAuditTrailEventIndex.EventName))
                .Column<int>(nameof(ContentAuditTrailEventIndex.VersionNumber))
                .Column<bool>(nameof(ContentAuditTrailEventIndex.Published)));

            SchemaBuilder.AlterIndexTable<ContentAuditTrailEventIndex>(table => table
                .CreateIndex("IDX_ContentAuditTrailEventIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentType",
                    "EventName",
                    "VersionNumber",
                    "Published")
            );

            return 1;
        }
    }
}
