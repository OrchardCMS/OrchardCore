using OrchardCore.Data.Migration;
using OrchardCore.Indexing.Core.Indexes;
using YesSql.Sql;

namespace OrchardCore.Indexing.DataMigrations;

internal sealed class IndexingMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<IndexProfileIndex>(table => table
            .Column<string>("IndexProfileId", column => column.WithLength(26))
            .Column<string>("Name", column => column.WithLength(255))
            .Column<string>("IndexName", column => column.WithLength(255))
            .Column<string>("ProviderName", column => column.WithLength(50))
            .Column<string>("Type", column => column.WithLength(50))
        );

        await SchemaBuilder.AlterIndexTableAsync<IndexProfileIndex>(table => table
            .CreateIndex("IDX_IndexProfileIndex_DocumentId", "DocumentId", "Name")
        );

        return 1;
    }
}
