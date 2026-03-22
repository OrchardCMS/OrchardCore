using OrchardCore.ContentsTransfer.Indexes;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.ContentsTransfer.Migrations;

public class ContentTransferMigrations : DataMigration
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
}
