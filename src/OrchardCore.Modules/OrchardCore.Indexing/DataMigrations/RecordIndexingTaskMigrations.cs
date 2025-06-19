using OrchardCore.Data.Migration;

namespace OrchardCore.Indexing.DataMigrations;

internal sealed class RecordIndexingTaskMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateTableAsync("RecordIndexingTask", table => table
           .Column<int>("Id", col => col.PrimaryKey().Identity())
           .Column<string>("RecordId", c => c.WithLength(26))
           .Column<string>("Category", c => c.WithLength(50))
           .Column<DateTime>("CreatedUtc", col => col.NotNull())
           .Column<int>("Type")
        );

        await SchemaBuilder.AlterTableAsync("RecordIndexingTask", table => table
            .CreateIndex("IDX_RecordIndexingTask_RecordId_Category", "RecordId", "Category")
        );

        return 1;
    }
}
