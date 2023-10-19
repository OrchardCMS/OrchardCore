using System;
using OrchardCore.Data.Migration;

namespace OrchardCore.Indexing
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(nameof(IndexingTask), table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<string>("ContentItemId", c => c.WithLength(26))
                .Column<DateTime>("CreatedUtc", col => col.NotNull())
                .Column<int>("Type")
            );

            SchemaBuilder.AlterTable(nameof(IndexingTask), table => table
                .CreateIndex("IDX_IndexingTask_ContentItemId", "ContentItemId")
            );

            return 1;
        }
    }
}
