using Orchard.Data.Migration;
using Orchard.OpenId.Indexes;

namespace Orchard.OpenId
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(OpenIdApplicationIndex), table => table
                .Column<string>(nameof(OpenIdApplicationIndex.ClientId))
                .Column<string>(nameof(OpenIdApplicationIndex.LogoutRedirectUri))
            );

            SchemaBuilder.CreateMapIndexTable(nameof(OpenIdTokenIndex), table => table
                .Column<int>(nameof(OpenIdTokenIndex.AppId))
                .Column<string>(nameof(OpenIdTokenIndex.Subject))
                .Column<int>(nameof(OpenIdTokenIndex.TokenId)));

            return 1;
        }
    }
}