using Orchard.Data.Migration;
using Orchard.OpenId.Indexes;

namespace Orchard.OpenId
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(OpenIdApplicationIndex), table => table
                .Column<string>("ClientId")
                .Column<string>("LogoutRedirectUri")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(OpenIdTokenIndex), table => table
                .Column<int>("UserId")
                .Column<int>("AppId"));

            return 1;
        }
    }
}