using Orchard.Data.Migration;
using Orchard.Identity.Indexes;

namespace Orchard.Identity
{
    public class Migrations : DataMigrations
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(UserIndex), table => table
                .Column<string>("NormalizedUserName")
                .Column<string>("PasswordHash")
            );

            return 1;
        }
    }
}