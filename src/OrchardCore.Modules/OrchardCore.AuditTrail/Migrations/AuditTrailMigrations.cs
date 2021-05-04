using System;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.AuditTrail.Migrations
{
    public class AuditTrailMigrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AuditTrailMigrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(AuditTrailPart), part => part
                .Attachable()
                .WithDescription("Allows editors to enter a comment to be saved into the Audit Trail event when saving a content item."));

            SchemaBuilder.CreateMapIndexTable<AuditTrailEventIndex>(table => table
                .Column<string>(nameof(AuditTrailEventIndex.AuditTrailEventId), column => column.WithLength(26))
                .Column<string>(nameof(AuditTrailEventIndex.Category))
                .Column<DateTime>(nameof(AuditTrailEventIndex.CreatedUtc), column => column.Nullable())
                .Column<string>(nameof(AuditTrailEventIndex.EventFilterData))
                .Column<string>(nameof(AuditTrailEventIndex.EventFilterKey))
                .Column<string>(nameof(AuditTrailEventIndex.EventName))
                .Column<string>(nameof(AuditTrailEventIndex.UserName), column => column.Nullable().WithLength(255)),
                collection: AuditTrailEvent.Collection);

            SchemaBuilder.AlterIndexTable<AuditTrailEventIndex>(table => table
                .CreateIndex("IDX_AuditTrailEventIndex_DocumentId",
                    "DocumentId",
                    "AuditTrailEventId",
                    "Category",
                    "CreatedUtc",
                    "EventFilterData",
                    "EventFilterKey",
                    "EventName",
                    "UserName"),
                collection: AuditTrailEvent.Collection
            );

            return 1;
        }
    }
}
