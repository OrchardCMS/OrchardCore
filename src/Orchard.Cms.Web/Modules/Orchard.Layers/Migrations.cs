using Orchard.Data.Migration;
using Orchard.Layers.Indexes;

namespace Orchard.Layers
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