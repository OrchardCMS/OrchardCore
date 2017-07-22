using System;
using Orchard.Data.Migration;

namespace Orchard.Indexing
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateTable(nameof(IndexingTask), table => table
                .Column<int>(nameof(IndexingTask.Id), col => col.PrimaryKey().Identity())
                .Column<string>(nameof(IndexingTask.ContentItemId), c => c.WithLength(26))
                .Column<DateTime>(nameof(IndexingTask.CreatedUtc), col => col.NotNull())
                .Column<int>(nameof(IndexingTask.Type))
            );

            SchemaBuilder.AlterTable(nameof(IndexingTask), table => table
                .CreateIndex("IDX_IndexingTask_ContentItemId", "ContentItemId")
            );

            return 1;
        }
    }
}
