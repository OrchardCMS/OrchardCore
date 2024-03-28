using System;
using System.Threading.Tasks;
using OrchardCore.Data.Migration;

namespace OrchardCore.Indexing
{
    public class Migrations : DataMigration
    {
        public async Task<int> CreateAsync()
        {
            await SchemaBuilder.CreateTableAsync(nameof(IndexingTask), table => table
                .Column<int>("Id", column => column.PrimaryKey().Identity())
                .Column<string>("ContentItemId", column => column.NotNull().WithLength(26))
                .Column<DateTime>("CreatedUtc", column => column.NotNull())
                .Column<int>("Type")
            );

            await SchemaBuilder.AlterTableAsync(nameof(IndexingTask), table => table
                .CreateIndex("IDX_IndexingTask_ContentItemId", "ContentItemId")
            );

            return 1;
        }
    }
}
