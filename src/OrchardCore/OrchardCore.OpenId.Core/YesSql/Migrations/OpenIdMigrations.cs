using System;
using System.Threading.Tasks;
using OrchardCore.Data.Migration;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Models;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.OpenId.YesSql.Migrations
{
    public class OpenIdMigrations : DataMigration
    {
        private const string OpenIdTokenCollection = OpenIdToken.OpenIdCollection;
        private const string OpenIdAuthorizationCollection = OpenIdAuthorization.OpenIdCollection;
        private const string OpenIdApplicationCollection = OpenIdApplication.OpenIdCollection;
        private const string OpenIdScopeCollection = OpenIdScope.OpenIdCollection;

        private readonly ISession _session;

        public OpenIdMigrations(ISession session)
        {
            _session = session;
        }

        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<OpenIdApplicationIndex>(table => table
                .Column<string>("ApplicationId", column => column.WithLength(48))
                .Column<string>("ClientId", column => column.Unique()),
                collection: OpenIdApplicationCollection);

            SchemaBuilder.AlterIndexTable<OpenIdApplicationIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdApplication",
                    "DocumentId",
                    "ApplicationId",
                    "ClientId"),
                collection: OpenIdApplicationCollection
            );

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByLogoutUriIndex>(table => table
                .Column<string>("LogoutRedirectUri")
                .Column<int>("Count"),
                collection: OpenIdApplicationCollection);

            SchemaBuilder.AlterIndexTable<OpenIdAppByLogoutUriIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdAppByLogoutUri_LogoutRedirectUri", "LogoutRedirectUri"),
                collection: OpenIdApplicationCollection
            );

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByRedirectUriIndex>(table => table
                .Column<string>("RedirectUri")
                .Column<int>("Count"),
                collection: OpenIdApplicationCollection);

            SchemaBuilder.AlterIndexTable<OpenIdAppByRedirectUriIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdAppByRedirectUri_RedirectUri", "RedirectUri"),
                collection: OpenIdApplicationCollection
            );

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByRoleNameIndex>(table => table
                .Column<string>("RoleName")
                .Column<int>("Count"),
                collection: OpenIdApplicationCollection);

            SchemaBuilder.AlterIndexTable<OpenIdAppByRoleNameIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdAppByRoleName_RoleName", "RoleName"),
                collection: OpenIdApplicationCollection
            );

            SchemaBuilder.CreateMapIndexTable<OpenIdAuthorizationIndex>(table => table
                .Column<string>("AuthorizationId", column => column.WithLength(48))
                .Column<string>("ApplicationId", column => column.WithLength(48))
                .Column<string>("Status")
                .Column<string>("Subject")
                .Column<string>("Type")
                .Column<DateTime>("CreationDate"),
                collection: OpenIdAuthorizationCollection);

            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdAuthorization_ApplicationId",
                    "DocumentId",
                    "ApplicationId",
                    "Status",
                    "Subject"),
                collection: OpenIdAuthorizationCollection
            );

            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdAuthorization_AuthorizationId",
                    "DocumentId",
                    "AuthorizationId",
                    "Status",
                    "Type",
                    "CreationDate"),
                collection: OpenIdAuthorizationCollection
            );

            SchemaBuilder.CreateMapIndexTable<OpenIdScopeIndex>(table => table
                .Column<string>("Name", column => column.Unique())
                .Column<string>("ScopeId", column => column.WithLength(48)),
                collection: OpenIdScopeCollection);

            SchemaBuilder.AlterIndexTable<OpenIdScopeIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdScope",
                    "DocumentId",
                    "Name",
                    "ScopeId"),
                collection: OpenIdScopeCollection
            );

            SchemaBuilder.CreateReduceIndexTable<OpenIdScopeByResourceIndex>(table => table
                .Column<string>("Resource")
                .Column<int>("Count"),
                collection: OpenIdScopeCollection);

            SchemaBuilder.AlterIndexTable<OpenIdScopeByResourceIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdScopeByResource_Resource",
                    "Resource"),
                collection: OpenIdScopeCollection
            );

            SchemaBuilder.CreateMapIndexTable<OpenIdTokenIndex>(table => table
                .Column<string>("TokenId", column => column.WithLength(48))
                .Column<string>("ApplicationId", column => column.WithLength(48))
                .Column<string>("AuthorizationId", column => column.WithLength(48))
                .Column<DateTime>("ExpirationDate")
                .Column<string>("ReferenceId")
                .Column<string>("Status")
                .Column<string>("Subject")
                .Column<string>("Type")
                .Column<DateTime>("CreationDate"),
                collection: OpenIdTokenCollection);

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdToken_ApplicationId",
                    "DocumentId",
                    "ApplicationId",
                    "Status",
                    "Subject"),
                collection: OpenIdTokenCollection
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdToken_AuthorizationId",
                    "DocumentId",
                    "AuthorizationId",
                    "Status",
                    "Type",
                    "CreationDate",
                    "ExpirationDate"),
                collection: OpenIdTokenCollection
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdToken_TokenId",
                    "DocumentId",
                    "TokenId",
                    "ReferenceId"),
                collection: OpenIdTokenCollection
            );

            // Shortcut other migration steps on new content definition schemas.
            return 8;
        }

        // This code can be removed in a later version.
        public int UpdateFrom1()
        {
            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .AddColumn<string>("Type"));

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
                .Column<string>("LogoutRedirectUri")
                .Column<int>("Count"));

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByRedirectUriIndex>(table => table
                .Column<string>("RedirectUri")
                .Column<int>("Count"));

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByRoleNameIndex>(table => table
                .Column<string>("RoleName")
                .Column<int>("Count"));

            return 3;
        }

        // This code can be removed in a later version.
        public int UpdateFrom3()
        {
            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .AddColumn<DateTime>("CreationDate"));

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .AddColumn<DateTime>("CreationDate"));

            return 4;
        }

        // This code can be removed in a later version.
        public int UpdateFrom4()
        {
            SchemaBuilder.AlterIndexTable<OpenIdApplicationIndex>(table => table
                .CreateIndex("IDX_OpenIdApplicationIndex_DocumentId",
                    "DocumentId",
                    "ApplicationId",
                    "ClientId")
            );

            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .CreateIndex("IDX_OpenIdAuthorizationIndex_DocumentId_ApplicationId",
                    "DocumentId",
                    "ApplicationId",
                    "Status",
                    "Subject")
            );

            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .CreateIndex("IDX_OpenIdAuthorizationIndex_DocumentId_AuthorizationId",
                    "DocumentId",
                    "AuthorizationId",
                    "Status",
                    "Type",
                    "CreationDate")
            );

            SchemaBuilder.AlterIndexTable<OpenIdScopeIndex>(table => table
                .CreateIndex("IDX_OpenIdScopeIndex_DocumentId",
                    "DocumentId",
                    "Name",
                    "ScopeId")
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_ApplicationId",
                    "DocumentId",
                    "ApplicationId",
                    "Status",
                    "Subject")
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_AuthorizationId",
                    "DocumentId",
                    "AuthorizationId",
                    "Status",
                    "Type",
                    "CreationDate",
                    "ExpirationDate")
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_TokenId",
                    "DocumentId",
                    "TokenId",
                    "ReferenceId")
            );

            return 5;
        }

        // This code can be removed in a later version.
        public int UpdateFrom5()
        {
            SchemaBuilder.AlterIndexTable<OpenIdAppByLogoutUriIndex>(table => table
                .CreateIndex("IDX_OpenIdAppByLogoutUriIndex_LogoutRedirectUri", "LogoutRedirectUri")
            );

            SchemaBuilder.AlterIndexTable<OpenIdAppByRedirectUriIndex>(table => table
                .CreateIndex("IDX_OpenIdAppByRedirectUriIndex_RedirectUri", "RedirectUri")
            );

            SchemaBuilder.AlterIndexTable<OpenIdAppByRoleNameIndex>(table => table
                .CreateIndex("IDX_OpenIdAppByRoleNameIndex_RoleName", "RoleName")
            );

            SchemaBuilder.AlterIndexTable<OpenIdScopeByResourceIndex>(table => table
                .CreateIndex("IDX_OpenIdScopeByResourceIndex_Resource", "Resource")
            );

            return 6;
        }

        // This code can be removed in a later version.
        public async Task<int> UpdateFrom6Async()
        {
            // Create all index tables with the new collection value.
            SchemaBuilder.CreateMapIndexTable<OpenIdTokenIndex>(table => table
                .Column<string>("TokenId", column => column.WithLength(48))
                .Column<string>("ApplicationId", column => column.WithLength(48))
                .Column<string>("AuthorizationId", column => column.WithLength(48))
                .Column<DateTime>("ExpirationDate")
                .Column<string>("ReferenceId")
                .Column<string>("Status")
                .Column<string>("Subject")
                .Column<string>("Type")
                .Column<DateTime>("CreationDate"),
                collection: OpenIdTokenCollection);

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdToken_ApplicationId",
                    "DocumentId",
                    "ApplicationId",
                    "Status",
                    "Subject"),
                collection: OpenIdTokenCollection
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdToken_AuthorizationId",
                    "DocumentId",
                    "AuthorizationId",
                    "Status",
                    "Type",
                    "CreationDate",
                    "ExpirationDate"),
                collection: OpenIdTokenCollection
            );

            SchemaBuilder.AlterIndexTable<OpenIdTokenIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdToken_TokenId",
                    "DocumentId",
                    "TokenId",
                    "ReferenceId"),
                collection: OpenIdTokenCollection
            );

            SchemaBuilder.CreateMapIndexTable<OpenIdAuthorizationIndex>(table => table
                .Column<string>("AuthorizationId", column => column.WithLength(48))
                .Column<string>("ApplicationId", column => column.WithLength(48))
                .Column<string>("Status")
                .Column<string>("Subject")
                .Column<string>("Type")
                .Column<DateTime>("CreationDate"),
                collection: OpenIdAuthorizationCollection);

            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdAuthorization_ApplicationId",
                    "DocumentId",
                    "ApplicationId",
                    "Status",
                    "Subject"),
                collection: OpenIdAuthorizationCollection
            );

            SchemaBuilder.AlterIndexTable<OpenIdAuthorizationIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdAuthorization_AuthorizationId",
                    "DocumentId",
                    "AuthorizationId",
                    "Status",
                    "Type",
                    "CreationDate"),
                collection: OpenIdAuthorizationCollection
            );

            // Retrieve all existing tokens and authorizations from original Document table.
            var tokens = await _session.Query<OpenIdToken>().ListAsync();
            var authorizations = await _session.Query<OpenIdAuthorization>().ListAsync();

            // Enlist the old documents in the new collection and remove from the old collections.
            foreach (var token in tokens)
            {
                // Set the id to 0 or it will be considered an updated entity.
                token.Id = 0;
                _session.Save(token, collection: OpenIdTokenCollection);

                // Delete from the original collection.
                _session.Delete(token);
            }

            // Enlist the old documents in the new collection and remove from the old collections.
            foreach (var authorization in authorizations)
            {
                // Set the id to 0 or it will be considered an updated entity.
                authorization.Id = 0;
                _session.Save(authorization, collection: OpenIdTokenCollection);

                // Delete from the original collection.
                _session.Delete(authorization);
            }

            // This can be safely dropped here as the index provider now only writes to the new collection table.
            SchemaBuilder.DropMapIndexTable<OpenIdTokenIndex>();
            SchemaBuilder.DropMapIndexTable<OpenIdAuthorizationIndex>();

            return 7;
        }

        // This code can be removed in a later version.
        public async Task<int> UpdateFrom7Async()
        {
            // Create all index tables with the new collection value.  
            SchemaBuilder.CreateMapIndexTable<OpenIdApplicationIndex>(table => table
                .Column<string>("ApplicationId", column => column.WithLength(48))
                .Column<string>("ClientId", column => column.Unique()),
                collection: OpenIdApplicationCollection);

            SchemaBuilder.AlterIndexTable<OpenIdApplicationIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdApplication",
                    "DocumentId",
                    "ApplicationId",
                    "ClientId"),
                collection: OpenIdApplicationCollection
            );

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByLogoutUriIndex>(table => table
                .Column<string>("LogoutRedirectUri")
                .Column<int>("Count"),
                collection: OpenIdApplicationCollection);

            SchemaBuilder.AlterIndexTable<OpenIdAppByLogoutUriIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdAppByLogoutUri_LogoutRedirectUri", "LogoutRedirectUri"),
                collection: OpenIdApplicationCollection
            );

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByRedirectUriIndex>(table => table
                .Column<string>("RedirectUri")
                .Column<int>("Count"),
                collection: OpenIdApplicationCollection);

            SchemaBuilder.AlterIndexTable<OpenIdAppByRedirectUriIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdAppByRedirectUri_RedirectUri", "RedirectUri"),
                collection: OpenIdApplicationCollection
            );

            SchemaBuilder.CreateReduceIndexTable<OpenIdAppByRoleNameIndex>(table => table
                .Column<string>("RoleName")
                .Column<int>("Count"),
                collection: OpenIdApplicationCollection);

            SchemaBuilder.AlterIndexTable<OpenIdAppByRoleNameIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdAppByRoleName_RoleName", "RoleName"),
                collection: OpenIdApplicationCollection
            );

            SchemaBuilder.CreateMapIndexTable<OpenIdScopeIndex>(table => table
                .Column<string>("Name", column => column.Unique())
                .Column<string>("ScopeId", column => column.WithLength(48)),
                collection: OpenIdScopeCollection);

            SchemaBuilder.AlterIndexTable<OpenIdScopeIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdScope",
                    "DocumentId",
                    "Name",
                    "ScopeId"),
                collection: OpenIdScopeCollection
            );

            SchemaBuilder.CreateReduceIndexTable<OpenIdScopeByResourceIndex>(table => table
                .Column<string>("Resource")
                .Column<int>("Count"),
                collection: OpenIdScopeCollection);

            SchemaBuilder.AlterIndexTable<OpenIdScopeByResourceIndex>(table => table
                .CreateIndex("IDX_COL_OpenIdScopeByResource_Resource", "Resource"),
                collection: OpenIdScopeCollection
            );

            // Retrieve all existing applications and scopes from original Document table.
            var applications = await _session.Query<OpenIdApplication>().ListAsync();
            var scopes = await _session.Query<OpenIdScope>().ListAsync();

            // Enlist the old documents in the new collection and remove from the old collections.
            foreach (var application in applications)
            {
                // Set the id to 0 or it will be considered an updated entity.
                application.Id = 0;
                _session.Save(application, collection: OpenIdApplicationCollection);

                // Delete from the original collection.
                _session.Delete(application);
            }

            // Enlist the old documents in the new collection and remove from the old collections.
            foreach (var scope in scopes)
            {
                // Set the id to 0 or it will be considered an updated entity.
                scope.Id = 0;
                _session.Save(scope, collection: OpenIdScopeCollection);

                // Delete from the original collection.
                _session.Delete(scope);
            }

            // Flush the saved documents so that the old reduced indexes will be calculated
            // and commited to the transaction before they are then dropped.
            await _session.FlushAsync();

            // These can be safely dropped after flushing.
            SchemaBuilder.DropMapIndexTable<OpenIdApplicationIndex>();
            SchemaBuilder.DropReduceIndexTable<OpenIdAppByLogoutUriIndex>();
            SchemaBuilder.DropReduceIndexTable<OpenIdAppByRedirectUriIndex>();
            SchemaBuilder.DropReduceIndexTable<OpenIdAppByRoleNameIndex>();

            SchemaBuilder.DropMapIndexTable<OpenIdScopeIndex>();
            SchemaBuilder.DropReduceIndexTable<OpenIdScopeByResourceIndex>();

            return 8;
        }
    }
}
