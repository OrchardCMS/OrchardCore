using OrchardCore.Data.Migration;

namespace OrchardCore.Indexing.DataMigrations;

internal sealed class RecordIndexingTaskMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateTableAsync("RecordIndexingTask", table => table
           .Column<int>("Id", column => column.PrimaryKey().Identity())
           .Column<string>("RecordId", column => column.WithLength(26))
           .Column<string>("Category", column => column.WithLength(50))
           .Column<DateTime>("CreatedUtc", column => column.NotNull())
           .Column<int>("Type")
        );

        await SchemaBuilder.AlterTableAsync("RecordIndexingTask", table => table
            .CreateIndex("IDX_RecordIndexingTask_RecordId_Category", "RecordId", "Category")
        );

        return 1;
    }
}
