using OrchardCore.AdminTrees.Indexes;
using OrchardCore.Data.Migration;

namespace OrchardCore.AdminTrees
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(AdminTreeIndex), table => table
                .Column<string>("Name")
            );

            return 1;
        }

    }
}
