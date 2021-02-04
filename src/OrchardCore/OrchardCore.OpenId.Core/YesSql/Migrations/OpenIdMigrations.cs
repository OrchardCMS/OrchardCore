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

            SchemaBuilder.AlterIndexTable<OpenIdApplicationIndex>(table => table
                .CreateIndex("IDX_OpenIdApplicationIndex_DocumentId",
                    "DocumentId",
                    nameof(OpenIdApplicationIndex.ApplicationId),
                    nameof(OpenIdApplicationIndex.ClientId))
            );

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
                .Column<string>(nameof(OpenIdAuthorizationIndex.Type))
                .Column<DateTime>(nameof(OpenIdAuthorizationIndex.CreationDate)));

            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .CreateIndex("IDX_OpenIdAuthorizationIndex_DocumentId_ApplicationId",
                    "DocumentId",
                    nameof(OpenIdAuthorizationIndex.ApplicationId),
                    nameof(OpenIdAuthorizationIndex.Status),
                    nameof(OpenIdAuthorizationIndex.Subject))
            );

            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .CreateIndex("IDX_OpenIdAuthorizationIndex_DocumentId_AuthorizationId",
                    "DocumentId",
                    nameof(OpenIdAuthorizationIndex.AuthorizationId),
                    nameof(OpenIdAuthorizationIndex.Status),
                    nameof(OpenIdAuthorizationIndex.Type),
                    nameof(OpenIdAuthorizationIndex.CreationDate))
            );

            SchemaBuilder.CreateMapIndexTable<OpenIdScopeIndex>(table => table
                .Column<string>(nameof(OpenIdScopeIndex.Name), column => column.Unique())
                .Column<string>(nameof(OpenIdScopeIndex.ScopeId), column => column.WithLength(48)));

            SchemaBuilder.AlterIndexTable<OpenIdScopeIndex>(table => table
                .CreateIndex("IDX_OpenIdScopeIndex_DocumentId",
                    "DocumentId",
                    nameof(OpenIdScopeIndex.Name),
                    nameof(OpenIdScopeIndex.ScopeId))
            );

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
                .Column<string>(nameof(OpenIdTokenIndex.Type))
                .Column<DateTime>(nameof(OpenIdTokenIndex.CreationDate)));

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_ApplicationId",
                    "DocumentId",
                    nameof(OpenIdTokenIndex.ApplicationId),
                    nameof(OpenIdTokenIndex.Status),
                    nameof(OpenIdTokenIndex.Subject))
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_AuthorizationId",
                    "DocumentId",
                    nameof(OpenIdTokenIndex.AuthorizationId),
                    nameof(OpenIdTokenIndex.Status),
                    nameof(OpenIdTokenIndex.Type),
                    nameof(OpenIdTokenIndex.CreationDate),
                    nameof(OpenIdTokenIndex.ExpirationDate))
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_TokenId",
                    "DocumentId",
                    nameof(OpenIdTokenIndex.TokenId),
                    nameof(OpenIdTokenIndex.ReferenceId))
            );

            // Shortcut other migration steps on new content definition schemas.
            return 5;
        }

        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .AddColumn<string>(nameof(OpenIdTokenIndex.Type)));

            return 2;
        }

        private class OpenIdApplicationByPostLogoutRedirectUriIndex { }
        private class OpenIdApplicationByRedirectUriIndex { }
        private class OpenIdApplicationByRoleNameIndex { }

        // This code can be removed in a later version.
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

        // This code can be removed in a later version.
        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .AddColumn<DateTime>(nameof(OpenIdAuthorizationIndex.CreationDate)));

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .AddColumn<DateTime>(nameof(OpenIdTokenIndex.CreationDate)));

            return 4;
        }

        // This code can be removed in a later version.
        public int UpdateFrom4()
        {
            SchemaBuilder.AlterIndexTable<OpenIdApplicationIndex>(table => table
                .CreateIndex("IDX_OpenIdApplicationIndex_DocumentId",
                    "DocumentId",
                    nameof(OpenIdApplicationIndex.ApplicationId),
                    nameof(OpenIdApplicationIndex.ClientId))
            );

            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .CreateIndex("IDX_OpenIdAuthorizationIndex_DocumentId_ApplicationId",
                    "DocumentId",
                    nameof(OpenIdAuthorizationIndex.ApplicationId),
                    nameof(OpenIdAuthorizationIndex.Status),
                    nameof(OpenIdAuthorizationIndex.Subject))
            );

            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .CreateIndex("IDX_OpenIdAuthorizationIndex_DocumentId_AuthorizationId",
                    "DocumentId",
                    nameof(OpenIdAuthorizationIndex.AuthorizationId),
                    nameof(OpenIdAuthorizationIndex.Status),
                    nameof(OpenIdAuthorizationIndex.Type),
                    nameof(OpenIdAuthorizationIndex.CreationDate))
            );

            SchemaBuilder.AlterIndexTable<OpenIdScopeIndex>(table => table
                .CreateIndex("IDX_OpenIdScopeIndex_DocumentId",
                    "DocumentId",
                    nameof(OpenIdScopeIndex.Name),
                    nameof(OpenIdScopeIndex.ScopeId))
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_ApplicationId",
                    "DocumentId",
                    nameof(OpenIdTokenIndex.ApplicationId),
                    nameof(OpenIdTokenIndex.Status),
                    nameof(OpenIdTokenIndex.Subject))
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_AuthorizationId",
                    "DocumentId",
                    nameof(OpenIdTokenIndex.AuthorizationId),
                    nameof(OpenIdTokenIndex.Status),
                    nameof(OpenIdTokenIndex.Type),
                    nameof(OpenIdTokenIndex.CreationDate),
                    nameof(OpenIdTokenIndex.ExpirationDate))
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_TokenId",
                    "DocumentId",
                    nameof(OpenIdTokenIndex.TokenId),
                    nameof(OpenIdTokenIndex.ReferenceId))
            );

            return 5;
        }
    }
}
