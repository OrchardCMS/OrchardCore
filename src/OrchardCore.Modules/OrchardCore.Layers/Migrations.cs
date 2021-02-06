using OrchardCore.Data.Migration;
using OrchardCore.Layers.Indexes;
using YesSql.Sql;

namespace OrchardCore.Layers
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<LayerMetadataIndex>(table => table
               .Column<string>("Zone", c => c.WithLength(64))
            );

            SchemaBuilder.AlterIndexTable<LayerMetadataIndex>(table => table
                .CreateIndex("IDX_LayerMetadataIndex_DocumentId", "DocumentId", "Zone")
            );

            // Shortcut other migration steps on new content definition schemas.
            return 2;
        }

        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTable<LayerMetadataIndex>(table => table
                .CreateIndex("IDX_LayerMetadataIndex_DocumentId", "DocumentId", "Zone")
            );

            return 2;
        }
    }
}
