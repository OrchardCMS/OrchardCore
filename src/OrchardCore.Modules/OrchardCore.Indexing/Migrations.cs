using OrchardCore.Data.Migration;

namespace OrchardCore.Indexing;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateTableAsync(nameof(IndexingTask), table => table
            .Column<int>("Id", col => col.PrimaryKey().Identity())
            .Column<string>("ContentItemId", c => c.WithLength(26))
            .Column<DateTime>("CreatedUtc", col => col.NotNull())
            .Column<int>("Type")
        );

        await SchemaBuilder.AlterTableAsync(nameof(IndexingTask), table => table
            .CreateIndex("IDX_IndexingTask_ContentItemId", "ContentItemId")
        );

        return 1;
    }
}
