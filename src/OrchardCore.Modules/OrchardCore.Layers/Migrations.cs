using OrchardCore.Data.Migration;
using OrchardCore.Layers.Indexes;

namespace OrchardCore.Layers
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(typeof(LayerMetadataIndex), table => table
                .Column<string>("Zone", c => c.WithLength(64)),
                null
            );

            return 1;
        }
    }
}
