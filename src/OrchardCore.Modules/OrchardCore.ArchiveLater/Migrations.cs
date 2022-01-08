using System;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.ArchiveLater.Indexes;
using OrchardCore.ArchiveLater.Models;
using YesSql.Sql;

namespace OrchardCore.ArchiveLater
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
            _contentDefinitionManager.AlterPartDefinition(nameof(ArchiveLaterPart), builder => builder
                .Attachable()
                .WithDescription("Adds the ability to schedule content items to be archived at a given future date and time."));

            SchemaBuilder.CreateMapIndexTable<ArchiveLaterPartIndex>(table => table
                .Column<DateTime>(nameof(ArchiveLaterPartIndex.ScheduledArchiveDateTimeUtc))
            );

            SchemaBuilder.AlterIndexTable<ArchiveLaterPartIndex>(table => table
                .CreateIndex($"IDX_{nameof(ArchiveLaterPartIndex)}_{nameof(ArchiveLaterPartIndex.ScheduledArchiveDateTimeUtc)}",
                    "DocumentId",
                    nameof(ArchiveLaterPartIndex.ScheduledArchiveDateTimeUtc))
            );

            return 1;
        }
    }
}
