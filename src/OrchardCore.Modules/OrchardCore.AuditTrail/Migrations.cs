using System;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.AuditTrail
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(AuditTrailPart), part => part
                .Attachable()
                .WithDescription("Allows editors to enter a comment to be saved into the Audit Trail event when saving a content item."));

            SchemaBuilder.CreateMapIndexTable<AuditTrailEventIndex>(table => table
                .Column<string>(nameof(AuditTrailEventIndex.EventId), column => column.WithLength(26))
                .Column<string>(nameof(AuditTrailEventIndex.Category), column => column.WithLength(128))
                .Column<string>(nameof(AuditTrailEventIndex.EventName), column => column.WithLength(128))
                .Column<string>(nameof(AuditTrailEventIndex.CorrelationId), column => column.WithLength(26))
                .Column<string>(nameof(AuditTrailEventIndex.UserName), column => column.Nullable().WithLength(255))
                .Column<DateTime>(nameof(AuditTrailEventIndex.CreatedUtc), column => column.Nullable()),
                collection: AuditTrailEvent.Collection);

            SchemaBuilder.AlterIndexTable<AuditTrailEventIndex>(table => table
                .CreateIndex("IDX_AuditTrailEventIndex_DocumentId",
                    "DocumentId",
                    "EventId",
                    "Category",
                    "EventName",
                    "CorrelationId",
                    "UserName",
                    "CreatedUtc"
                    ),
                collection: AuditTrailEvent.Collection
            );

            return 1;
        }
    }
}
