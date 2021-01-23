using System;
using OrchardCore.Data.Migration;
using OrchardCore.OpenId.YesSql.Indexes;
using YesSql.Sql;

namespace OrchardCore.OpenId.YesSql.Migrations
{
    public class OpenIdMigrations : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<OpenIdApplicationIndex>(table => table
                .Column<string>(nameof(OpenIdApplicationIndex.ApplicationId), column => column.WithLength(48))
                .Column<string>(nameof(OpenIdApplicationIndex.ClientId), column => column.Unique()));

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByLogoutUriIndex>(table => table
                .Column<string>(nameof(OpenIdAppByLogoutUriIndex.LogoutRedirectUri))
                .Column<int>(nameof(OpenIdAppByLogoutUriIndex.Count)));

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByRedirectUriIndex>(table => table
                .Column<string>(nameof(OpenIdAppByRedirectUriIndex.RedirectUri))
                .Column<int>(nameof(OpenIdAppByRedirectUriIndex.Count)));

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByRoleNameIndex>(table => table
                .Column<string>(nameof(OpenIdAppByRoleNameIndex.RoleName))
                .Column<int>(nameof(OpenIdAppByRoleNameIndex.Count)));

            SchemaBuilder.CreateMapIndexTable<OpenIdAuthorizationIndex>(table => table
                .Column<string>(nameof(OpenIdAuthorizationIndex.AuthorizationId), column => column.WithLength(48))
                .Column<string>(nameof(OpenIdAuthorizationIndex.ApplicationId), column => column.WithLength(48))
                .Column<string>(nameof(OpenIdAuthorizationIndex.Status))
                .Column<string>(nameof(OpenIdAuthorizationIndex.Subject))
                .Column<string>(nameof(OpenIdAuthorizationIndex.Type)));

            SchemaBuilder.CreateMapIndexTable<OpenIdScopeIndex>(table => table
                .Column<string>(nameof(OpenIdScopeIndex.Name), column => column.Unique())
                .Column<string>(nameof(OpenIdScopeIndex.ScopeId), column => column.WithLength(48)));

            SchemaBuilder.CreateReduceIndexTable<OpenIdScopeByResourceIndex>(table => table
                .Column<string>(nameof(OpenIdScopeByResourceIndex.Resource))
                .Column<int>(nameof(OpenIdScopeByResourceIndex.Count)));

            SchemaBuilder.CreateMapIndexTable<OpenIdTokenIndex>(table => table
                .Column<string>(nameof(OpenIdTokenIndex.TokenId), column => column.WithLength(48))
                .Column<string>(nameof(OpenIdTokenIndex.ApplicationId), column => column.WithLength(48))
                .Column<string>(nameof(OpenIdTokenIndex.AuthorizationId), column => column.WithLength(48))
                .Column<DateTime>(nameof(OpenIdTokenIndex.ExpirationDate))
                .Column<string>(nameof(OpenIdTokenIndex.ReferenceId))
                .Column<string>(nameof(OpenIdTokenIndex.Status))
                .Column<string>(nameof(OpenIdTokenIndex.Subject))
                .Column<string>(nameof(OpenIdTokenIndex.Type)));

            return 3;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .AddColumn<string>(nameof(OpenIdTokenIndex.Type)));

            return 2;
        }

        private class OpenIdApplicationByPostLogoutRedirectUriIndex { }
        private class OpenIdApplicationByRedirectUriIndex { }
        private class OpenIdApplicationByRoleNameIndex { }

        public int UpdateFrom2()
        {
            SchemaBuilder.DropReduceIndexTable<OpenIdApplicationByPostLogoutRedirectUriIndex>(null);
            SchemaBuilder.DropReduceIndexTable<OpenIdApplicationByRedirectUriIndex>(null);
            SchemaBuilder.DropReduceIndexTable<OpenIdApplicationByRoleNameIndex>(null);

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByLogoutUriIndex>(table => table
                .Column<string>(nameof(OpenIdAppByLogoutUriIndex.LogoutRedirectUri))
                .Column<int>(nameof(OpenIdAppByLogoutUriIndex.Count)));

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByRedirectUriIndex>(table => table
                .Column<string>(nameof(OpenIdAppByRedirectUriIndex.RedirectUri))
                .Column<int>(nameof(OpenIdAppByRedirectUriIndex.Count)));

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByRoleNameIndex>(table => table
                .Column<string>(nameof(OpenIdAppByRoleNameIndex.RoleName))
                .Column<int>(nameof(OpenIdAppByRoleNameIndex.Count)));

            return 3;
        }

        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .AddColumn<DateTime>(nameof(OpenIdAuthorizationIndex.CreationDate)));

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .AddColumn<DateTime>(nameof(OpenIdTokenIndex.CreationDate)));

            return 4;
        }
    }
}
