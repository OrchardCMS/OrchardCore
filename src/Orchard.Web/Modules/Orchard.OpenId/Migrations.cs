using Orchard.Data.Migration;
using Orchard.OpenId.Indexes;

namespace Orchard.OpenId
{
    public class Migrations : DataMigrations
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(OpenIdApplicationIndex), table => table
                .Column<string>("ClientId")
                .Column<string>("LogoutRedirectUri")
            );

            SchemaBuilder.CreateMapIndexTable(nameof(UserOpenIdTokenIndex), table => table
                .Column<int>("UserId"));

            return 1;
        }
    }
}