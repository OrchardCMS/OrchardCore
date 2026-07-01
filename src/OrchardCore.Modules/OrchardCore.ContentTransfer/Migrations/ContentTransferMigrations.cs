using OrchardCore.ContentTransfer.Indexes;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.ContentTransfer.Migrations;

public sealed class ContentTransferMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<ContentTransferEntryIndex>(table => table
            .Column<string>("EntryId", column => column.WithLength(26))
            .Column<string>("Status", column => column.NotNull().WithLength(25))
            .Column<DateTime>("CreatedUtc", column => column.NotNull())
            .Column<string>("ContentType", column => column.WithLength(255))
            .Column<string>("Owner", column => column.WithLength(26))
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentTransferEntryIndex>(table => table
            .CreateIndex("IDX_ContentTransferEntryIndex_DocumentId",
                "DocumentId",
                "EntryId",
                "Status",
                "CreatedUtc",
                "ContentType",
                "Owner")
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentTransferEntryIndex>(table => table
            .CreateIndex("IDX_ContentTransferEntryIndex_Status",
                "Status",
                "CreatedUtc",
                "DocumentId",
                "ContentType",
                "Owner")
        );

        return 1;
    }

    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<ContentTransferEntryIndex>(table => table
            .AddColumn<int>("Direction", column => column.NotNull().WithDefault(0))
        );

        await SchemaBuilder.AlterIndexTableAsync<ContentTransferEntryIndex>(table => table
            .CreateIndex("IDX_ContentTransferEntryIndex_Direction",
                "Direction",
                "Status",
                "CreatedUtc",
                "DocumentId")
        );

        return 2;
    }
}
