using System;
using OrchardCore.ContentsTransfer.Indexes;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.ContentsTransfer.Migrations;

public class ContentTransferMigrations : DataMigration
{
    public int Create()
    {
        SchemaBuilder.CreateMapIndexTable<ContentTransferEntryIndex>(table => table
            .Column<string>("EntryId", column => column.WithLength(26))
            .Column<string>("Status", column => column.NotNull().WithLength(25))
            .Column<DateTime>("CreatedUtc", column => column.NotNull())
            .Column<string>("ContentType", column => column.WithLength(255))
            .Column<string>("Owner", column => column.WithLength(26))
        );

        SchemaBuilder.AlterIndexTable<ContentTransferEntryIndex>(table => table
            .CreateIndex("IDX_ContentTransferEntryIndex_DocumentId",
                "DocumentId",
                "EntryId",
                "Status",
                "CreatedUtc",
                "ContentType",
                "Owner")
        );

        SchemaBuilder.AlterIndexTable<ContentTransferEntryIndex>(table => table
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
