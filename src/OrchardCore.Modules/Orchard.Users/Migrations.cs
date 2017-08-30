using Orchard.Data.Migration;
using Orchard.Users.Indexes;

namespace Orchard.Users
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(UserIndex), table => table
                .Column<string>("NormalizedUserName")
                .Column<string>("NormalizedEmail")
            );

            SchemaBuilder.CreateReduceIndexTable(nameof(UserByRoleNameIndex), table => table
                .Column<string>("RoleName")
                .Column<int>("Count")
            );

            return 1;
        }
    }
}