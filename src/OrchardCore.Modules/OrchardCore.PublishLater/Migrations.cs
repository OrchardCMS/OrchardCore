using System;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
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

    public int Create()
    {
        _contentDefinitionManager.AlterPartDefinition(nameof(PublishLaterPart), builder => builder
            .Attachable()
            .WithDescription("Adds the ability to schedule content items to be published at a given future date and time."));

        SchemaBuilder.CreateMapIndexTable<PublishLaterPartIndex>(table => table
            .Column<string>(nameof(PublishLaterPartIndex.ContentItemId))
            .Column<DateTime>(nameof(PublishLaterPartIndex.ScheduledPublishDateTimeUtc))
            .Column<bool>(nameof(PublishLaterPartIndex.Published))
            .Column<bool>(nameof(PublishLaterPartIndex.Latest))
        );

        SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table => table
            .CreateIndex($"IDX_{nameof(PublishLaterPartIndex)}_{nameof(ContentItemIndex.DocumentId)}",
                "Id",
                nameof(ContentItemIndex.DocumentId),
                nameof(PublishLaterPartIndex.ContentItemId),
                nameof(PublishLaterPartIndex.ScheduledPublishDateTimeUtc),
                nameof(PublishLaterPartIndex.Published),
                nameof(PublishLaterPartIndex.Latest))
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
            .CreateIndex($"IDX_{nameof(PublishLaterPartIndex)}_{nameof(PublishLaterPartIndex.ScheduledPublishDateTimeUtc)}",
                nameof(ContentItemIndex.DocumentId),
                nameof(PublishLaterPartIndex.ScheduledPublishDateTimeUtc))
        );

        return 2;
    }

    public int UpdateFrom2()
    {
        SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table =>
        {
            table.AddColumn<string>(nameof(PublishLaterPartIndex.ContentItemId));
            table.AddColumn<bool>(nameof(PublishLaterPartIndex.Published));
            table.AddColumn<bool>(nameof(PublishLaterPartIndex.Latest));
        });

        SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table =>
        {
            table.DropIndex($"IDX_{nameof(PublishLaterPartIndex)}_{nameof(PublishLaterPartIndex.ScheduledPublishDateTimeUtc)}");
        });

        SchemaBuilder.AlterIndexTable<PublishLaterPartIndex>(table =>
        {
            table.CreateIndex($"IDX_{nameof(PublishLaterPartIndex)}_{nameof(PublishLaterPartIndex.ScheduledPublishDateTimeUtc)}",
                "Id",
                nameof(ContentItemIndex.DocumentId),
                nameof(PublishLaterPartIndex.ContentItemId),
                nameof(PublishLaterPartIndex.ScheduledPublishDateTimeUtc),
                nameof(PublishLaterPartIndex.Published),
                nameof(PublishLaterPartIndex.Latest));
        });

        return 3;
    }
}
