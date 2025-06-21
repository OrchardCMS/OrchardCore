using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.PublishLater.Indexes;
using YesSql.Sql;

namespace OrchardCore.PublishLater;

public sealed class Migrations : DataMigration
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
            .WithDescription("Adds the ability to schedule content items to be published at a given future date and time.")).ConfigureAwait(false);

        await SchemaBuilder.CreateMapIndexTableAsync<PublishLaterPartIndex>(table => table
            .Column<string>("ContentItemId")
            .Column<DateTime>("ScheduledPublishDateTimeUtc")
            .Column<bool>("Published")
            .Column<bool>("Latest")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<PublishLaterPartIndex>(table => table
            .CreateIndex("IDX_PublishLaterPartIndex_DocumentId",
                "Id",
                "DocumentId",
                "ContentItemId",
                "ScheduledPublishDateTimeUtc",
                "Published",
                "Latest")
        ).ConfigureAwait(false);

        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        // The 'ScheduledPublishDateTimeUtc' column and related index are kept on existing databases,
        // this because dropping an index and altering a column don't work on all providers.

        await SchemaBuilder.AlterIndexTableAsync<PublishLaterPartIndex>(table => table
            .AddColumn<DateTime>(nameof(PublishLaterPartIndex.ScheduledPublishDateTimeUtc))
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<PublishLaterPartIndex>(table => table
            .CreateIndex($"IDX_PublishLaterPartIndex_ScheduledPublishDateTimeUtc",
                "DocumentId",
                "ScheduledPublishDateTimeUtc")
        ).ConfigureAwait(false);

        return 2;
    }

    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<PublishLaterPartIndex>(table =>
        {
            table.AddColumn<string>("ContentItemId");
            table.AddColumn<bool>("Published");
            table.AddColumn<bool>("Latest");
        }).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<PublishLaterPartIndex>(table =>
        {
            table.DropIndex("IDX_PublishLaterPartIndex_ScheduledPublishDateTimeUtc");
        }).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<PublishLaterPartIndex>(table =>
        {
            table.CreateIndex("IDX_PublishLaterPartIndex_ScheduledPublishDateTimeUtc",
                "Id",
                "DocumentId",
                "ContentItemId",
                "ScheduledPublishDateTimeUtc",
                "Published",
                "Latest");
        }).ConfigureAwait(false);

        return 3;
    }
}
