using System;
using System.Threading.Tasks;
using OrchardCore.ArchiveLater.Indexes;
using OrchardCore.ArchiveLater.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
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

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(ArchiveLaterPart), builder => builder
            .Attachable()
            .WithDescription("Adds the ability to schedule content items to be archived at a given future date and time."));

        SchemaBuilder.CreateMapIndexTable<ArchiveLaterPartIndex>(table => table
            .Column<string>("ContentItemId")
            .Column<DateTime>("ScheduledArchiveDateTimeUtc")
            .Column<bool>("Published")
            .Column<bool>("Latest")
        );

        SchemaBuilder.AlterIndexTable<ArchiveLaterPartIndex>(table => table
            .CreateIndex("IDX_ArchiveLaterPartIndex_DocumentId",
                "Id",
                "DocumentId",
                "ContentItemId",
                "ScheduledArchiveDateTimeUtc",
                "Published",
                "Latest")
        );

        return 1;
    }
}
