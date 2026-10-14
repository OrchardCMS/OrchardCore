using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.DataPipelines.SharedLinks;

public sealed class SharedLinkMigrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<SharedLinkIndex>(table => table
            .Column<string>("LinkId", column => column.WithLength(SharedLinkIndex.MaxIdLength))
            .Column<string>("Kind", column => column.WithLength(SharedLinkIndex.MaxKindLength))
            .Column<string>("Title", column => column.Nullable().WithLength(SharedLinkIndex.MaxTitleLength))
            .Column<string>("SourceName", column => column.Nullable().WithLength(SharedLinkIndex.MaxTitleLength))
            .Column<DateTime>("CreatedUtc")
            .Column<DateTime>("ExpiresUtc")
        );

        await SchemaBuilder.AlterIndexTableAsync<SharedLinkIndex>(table => table
            .CreateIndex("IDX_SharedLinkIndex_DocumentId", "DocumentId", "LinkId", "ExpiresUtc")
        );

        return 1;
    }
}
