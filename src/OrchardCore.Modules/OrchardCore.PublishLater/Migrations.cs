using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.PublishLater.Indexes;
using OrchardCore.PublishLater.Models;
using YesSql.Sql;

namespace OrchardCore.PublishLater;

public class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync("PublishLaterPart", builder => builder
            .Attachable()
            .WithDescription("Adds the ability to schedule content items to be published at a given future date and time."));

        SchemaBuilder.CreateMapIndexTable<PublishLaterPartIndex>(table => table
            .Column<string>("ContentItemId")
            .Column<DateTime>("ScheduledPublishDateTimeUtc")
            .Column<bool>("Published")
            .Column<bool>("Latest")
        );

        SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table => table
            .CreateIndex("IDX_PublishLaterPartIndex_DocumentId",
                "Id",
                "DocumentId",
                "ContentItemId",
                "ScheduledPublishDateTimeUtc",
                "Published",
                "Latest")
        );

        return 3;
    }

    // This code can be removed in a later version.
    public int UpdateFrom1()
    {
        // The 'ScheduledPublishUtc' column and related index are kept on existing databases,
        // this because dropping an index and altering a column don't work on all providers.

        SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table => table
            .AddColumn<DateTime>(nameof(PublishLaterPartIndex.ScheduledPublishDateTimeUtc))
        );

        SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table => table
            .CreateIndex($"IDX_PublishLaterPartIndex_ScheduledPublishDateTimeUtc",
                "DocumentId",
                "ScheduledPublishDateTimeUtc")
        );

        return 2;
    }

    public int UpdateFrom2()
    {
        SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table =>
        {
            table.AddColumn<string>("ContentItemId");
            table.AddColumn<bool>("Published");
            table.AddColumn<bool>("Latest");
        });

        SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table =>
        {
            table.DropIndex("IDX_PublishLaterPartIndex_ScheduledPublishDateTimeUtc");
        });

        SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table =>
        {
            table.CreateIndex("IDX_PublishLaterPartIndex_ScheduledPublishDateTimeUtc",
                "Id",
                "DocumentId",
                "ContentItemId",
                "ScheduledPublishDateTimeUtc",
                "Published",
                "Latest");
        });

        return 3;
    }
}
