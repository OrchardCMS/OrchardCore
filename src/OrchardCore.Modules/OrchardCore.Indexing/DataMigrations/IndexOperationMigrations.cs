using OrchardCore.Data.Migration;
using OrchardCore.Indexing.Core.Indexes;
using YesSql.Sql;

namespace OrchardCore.Indexing.DataMigrations;

internal sealed class IndexOperationMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<IndexOperationIndex>(table => table
            .Column<string>(nameof(IndexOperationIndex.OperationId), column => column.WithLength(32)));
        await SchemaBuilder.AlterIndexTableAsync<IndexOperationIndex>(table => table
            .CreateIndex("IDX_IndexOperationIndex_OperationId", nameof(IndexOperationIndex.OperationId)));
        return 1;
    }
}
