using System;
using OrchardCore.ArchiveLater.Indexes;
using OrchardCore.ArchiveLater.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.ArchiveLater;

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
            .Column<string>(nameof(ArchiveLaterPartIndex.ContentItemId))
            .Column<bool>(nameof(ArchiveLaterPartIndex.Latest))
            .Column<bool>(nameof(ArchiveLaterPartIndex.Published))
            .Column<DateTime>(nameof(ArchiveLaterPartIndex.ScheduledArchiveDateTimeUtc))
        );

        SchemaBuilder.AlterIndexTable<ArchiveLaterPartIndex>(table => table
            .CreateIndex($"IDX_{nameof(ArchiveLaterPartIndex)}_{nameof(ContentItemIndex.DocumentId)}",
                "Id",
                nameof(ContentItemIndex.DocumentId),
                nameof(ArchiveLaterPartIndex.ContentItemId),
                nameof(ArchiveLaterPartIndex.Latest),
                nameof(ArchiveLaterPartIndex.Published),
                nameof(ArchiveLaterPartIndex.ScheduledArchiveDateTimeUtc))
        );

        return 1;
    }
}
