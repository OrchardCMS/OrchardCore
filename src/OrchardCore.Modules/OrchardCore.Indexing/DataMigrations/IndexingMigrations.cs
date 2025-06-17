using OrchardCore.Data.Migration;
using OrchardCore.Indexing.Core.Indexes;
using YesSql.Sql;

namespace OrchardCore.Indexing.DataMigrations;

internal sealed class IndexingMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<IndexProfileIndex>(table => table
            .Column<string>("IndexProfileId", c => c.WithLength(26))
            .Column<string>("Name", c => c.WithLength(255))
            .Column<string>("IndexName", c => c.WithLength(255))
            .Column<string>("ProviderName", c => c.WithLength(50))
            .Column<string>("Type", c => c.WithLength(50))
        );

        await SchemaBuilder.AlterTableAsync("IndexProfileIndex", table => table
            .CreateIndex("IDX_IndexProfileIndex_DocumentId", "DocumentId", "Name")
        );

        await SchemaBuilder.AlterTableAsync("IndexProfileIndex", table => table
            .CreateIndex("IDX_IndexProfileIndex_ProviderName_Type", "ProviderName", "Type")
        );

        return 1;
    }
}
