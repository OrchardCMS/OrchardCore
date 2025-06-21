using OrchardCore.Data.Migration;
using OrchardCore.Layers.Indexes;
using YesSql.Sql;

namespace OrchardCore.Layers;

public sealed class Migrations : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<LayerMetadataIndex>(table => table
           .Column<string>("Zone", c => c.WithLength(64))
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<LayerMetadataIndex>(table => table
            .CreateIndex("IDX_LayerMetadataIndex_DocumentId",
            "DocumentId",
            "Zone")
        ).ConfigureAwait(false);

        // Shortcut other migration steps on new content definition schemas.
        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<LayerMetadataIndex>(table => table
            .CreateIndex("IDX_LayerMetadataIndex_DocumentId",
            "DocumentId",
            "Zone")
        ).ConfigureAwait(false);

        // Migration was cleaned up in version 2.0.
        // Jump to step 3 during create.
        return 3;
    }
}
