using System;
using OrchardCore.Data.Migration;
using OrchardCore.OpenId.Indexes;

namespace OrchardCore.OpenId
{
    public class Migrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable(nameof(OpenIdApplicationIndex), table => table
                .Column<string>(nameof(OpenIdApplicationIndex.ClientId)));

            SchemaBuilder.CreateReduceIndexTable(nameof(OpenIdApplicationByPostLogoutRedirectUriIndex), table => table
                .Column<string>(nameof(OpenIdApplicationByPostLogoutRedirectUriIndex.PostLogoutRedirectUri))
                .Column<int>(nameof(OpenIdApplicationByPostLogoutRedirectUriIndex.Count)));

            SchemaBuilder.CreateReduceIndexTable(nameof(OpenIdApplicationByRedirectUriIndex), table => table
                .Column<string>(nameof(OpenIdApplicationByRedirectUriIndex.RedirectUri))
                .Column<int>(nameof(OpenIdApplicationByPostLogoutRedirectUriIndex.Count)));

            SchemaBuilder.CreateReduceIndexTable(nameof(OpenIdApplicationByRoleNameIndex), table => table
                .Column<string>(nameof(OpenIdApplicationByRoleNameIndex.RoleName))
                .Column<int>(nameof(OpenIdApplicationByRoleNameIndex.Count)));

            SchemaBuilder.CreateMapIndexTable(nameof(OpenIdTokenIndex), table => table
                .Column<int>(nameof(OpenIdTokenIndex.ApplicationId))
                .Column<int>(nameof(OpenIdTokenIndex.AuthorizationId))
                .Column<DateTimeOffset>(nameof(OpenIdTokenIndex.ExpirationDate))
                .Column<string>(nameof(OpenIdTokenIndex.ReferenceId))
                .Column<string>(nameof(OpenIdTokenIndex.Status))
                .Column<string>(nameof(OpenIdTokenIndex.Subject))
                .Column<int>(nameof(OpenIdTokenIndex.TokenId)));

            return 1;
        }
    }
}