using Orchard.Data.Migration;
using Orchard.Roles.Indexes;

namespace Orchard.Roles
{
    public class Migrations : DataMigrations
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(RoleIndex), table => table
                .Column<string>("NormalizedRoleName")
            );

            return 1;
        }
    }
}