using OrchardCore.Data.Migration;
using OrchardCore.Layers.Indexes;

namespace OrchardCore.Layers
{
    public class Migrations : DataMigration
    {
		public int Create()
		{
			SchemaBuilder.CreateMapIndexTable(nameof(LayerMetadataIndex), table => table
				.Column<string>("Zone", c => c.WithLength(64))
			);

			return 1;
		}
	}
}