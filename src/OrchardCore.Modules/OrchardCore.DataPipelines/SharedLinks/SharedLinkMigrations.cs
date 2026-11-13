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
            .Column<string>("ResourceId", column => column.Nullable().WithLength(SharedLinkIndex.MaxResourceIdLength))
            .Column<string>("Title", column => column.Nullable().WithLength(SharedLinkIndex.MaxTitleLength))
            .Column<string>("SourceName", column => column.Nullable().WithLength(SharedLinkIndex.MaxTitleLength))
            .Column<DateTime>("CreatedUtc")
            .Column<DateTime>("ExpiresUtc")
        );

        await SchemaBuilder.AlterIndexTableAsync<SharedLinkIndex>(table => table
            .CreateIndex("IDX_SharedLinkIndex_DocumentId", "DocumentId", "LinkId", "ExpiresUtc")
        );

        await SchemaBuilder.AlterIndexTableAsync<SharedLinkIndex>(table => table
            .CreateIndex("IDX_SharedLinkIndex_Resource", "DocumentId", "Kind", "ResourceId")
        );

        // The counters of the uses, out of the index, which YesSql rewrites when a link is saved.
        await SchemaBuilder.CreateTableAsync(SharedLinkUseCounter.TableName, table => table
            .Column<string>("LinkId", column => column.PrimaryKey().WithLength(SharedLinkIndex.MaxIdLength))
            .Column<int>("Uses", column => column.NotNull().WithDefault(0))
        );

        return 2;
    }

    // Lists the links of a resource.
    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<SharedLinkIndex>(table => table
            .AddColumn<string>("ResourceId", column => column.Nullable().WithLength(SharedLinkIndex.MaxResourceIdLength))
        );

        await SchemaBuilder.AlterIndexTableAsync<SharedLinkIndex>(table => table
            .CreateIndex("IDX_SharedLinkIndex_Resource", "DocumentId", "Kind", "ResourceId")
        );

        return 2;
    }
}
