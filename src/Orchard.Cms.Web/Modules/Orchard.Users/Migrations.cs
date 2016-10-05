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

            return 1;
        }
    }
}