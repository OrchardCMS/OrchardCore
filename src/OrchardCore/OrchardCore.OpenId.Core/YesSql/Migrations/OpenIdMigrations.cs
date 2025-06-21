using OrchardCore.Data.Migration;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Models;
using YesSql;
using YesSql.Sql;

namespace OrchardCore.OpenId.YesSql.Migrations;

public sealed class OpenIdMigrations : DataMigration
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

    public async Task<int> CreateAsync()
    {
        await SchemaBuilder.CreateMapIndexTableAsync<OpenIdApplicationIndex>(table => table
            .Column<string>("ApplicationId", column => column.WithLength(48))
            .Column<string>("ClientId", column => column.Unique()),
            collection: OpenIdApplicationCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdApplicationIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdApplication",
                "DocumentId",
                "ApplicationId",
                "ClientId"),
            collection: OpenIdApplicationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdAppByLogoutUriIndex>(table => table
            .Column<string>("LogoutRedirectUri")
            .Column<int>("Count"),
            collection: OpenIdApplicationCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAppByLogoutUriIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdAppByLogoutUri_LogoutRedirectUri", "LogoutRedirectUri"),
            collection: OpenIdApplicationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdAppByRedirectUriIndex>(table => table
            .Column<string>("RedirectUri")
            .Column<int>("Count"),
            collection: OpenIdApplicationCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAppByRedirectUriIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdAppByRedirectUri_RedirectUri", "RedirectUri"),
            collection: OpenIdApplicationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdAppByRoleNameIndex>(table => table
            .Column<string>("RoleName")
            .Column<int>("Count"),
            collection: OpenIdApplicationCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAppByRoleNameIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdAppByRoleName_RoleName", "RoleName"),
            collection: OpenIdApplicationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateMapIndexTableAsync<OpenIdAuthorizationIndex>(table => table
            .Column<string>("AuthorizationId", column => column.WithLength(48))
            .Column<string>("ApplicationId", column => column.WithLength(48))
            .Column<string>("Status")
            .Column<string>("Subject")
            .Column<string>("Type")
            .Column<DateTime>("CreationDate"),
            collection: OpenIdAuthorizationCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAuthorizationIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdAuthorization_ApplicationId",
                "DocumentId",
                "ApplicationId",
                "Status",
                "Subject"),
            collection: OpenIdAuthorizationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAuthorizationIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdAuthorization_AuthorizationId",
                "DocumentId",
                "AuthorizationId",
                "Status",
                "Type",
                "CreationDate"),
            collection: OpenIdAuthorizationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateMapIndexTableAsync<OpenIdScopeIndex>(table => table
            .Column<string>("Name", column => column.Unique())
            .Column<string>("ScopeId", column => column.WithLength(48)),
            collection: OpenIdScopeCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdScopeIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdScope",
                "DocumentId",
                "Name",
                "ScopeId"),
            collection: OpenIdScopeCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdScopeByResourceIndex>(table => table
            .Column<string>("Resource")
            .Column<int>("Count"),
            collection: OpenIdScopeCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdScopeByResourceIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdScopeByResource_Resource",
                "Resource"),
            collection: OpenIdScopeCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateMapIndexTableAsync<OpenIdTokenIndex>(table => table
            .Column<string>("TokenId", column => column.WithLength(48))
            .Column<string>("ApplicationId", column => column.WithLength(48))
            .Column<string>("AuthorizationId", column => column.WithLength(48))
            .Column<DateTime>("ExpirationDate")
            .Column<string>("ReferenceId")
            .Column<string>("Status")
            .Column<string>("Subject")
            .Column<string>("Type")
            .Column<DateTime>("CreationDate"),
            collection: OpenIdTokenCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdToken_ApplicationId",
                "DocumentId",
                "ApplicationId",
                "Status",
                "Subject"),
            collection: OpenIdTokenCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdToken_AuthorizationId",
                "DocumentId",
                "AuthorizationId",
                "Status",
                "Type",
                "CreationDate",
                "ExpirationDate"),
            collection: OpenIdTokenCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdToken_TokenId",
                "DocumentId",
                "TokenId",
                "ReferenceId"),
            collection: OpenIdTokenCollection
        ).ConfigureAwait(false);

        // Shortcut other migration steps on new content definition schemas.
        return 8;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom1Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .AddColumn<string>("Type")).ConfigureAwait(false);

        return 2;
    }

    private sealed class OpenIdApplicationByPostLogoutRedirectUriIndex { }
    private sealed class OpenIdApplicationByRedirectUriIndex { }
    private sealed class OpenIdApplicationByRoleNameIndex { }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom2Async()
    {
        await SchemaBuilder.DropReduceIndexTableAsync<OpenIdApplicationByPostLogoutRedirectUriIndex>(null).ConfigureAwait(false);
        await SchemaBuilder.DropReduceIndexTableAsync<OpenIdApplicationByRedirectUriIndex>(null).ConfigureAwait(false);
        await SchemaBuilder.DropReduceIndexTableAsync<OpenIdApplicationByRoleNameIndex>(null).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdAppByLogoutUriIndex>(table => table
            .Column<string>("LogoutRedirectUri")
            .Column<int>("Count")).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdAppByRedirectUriIndex>(table => table
            .Column<string>("RedirectUri")
            .Column<int>("Count")).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdAppByRoleNameIndex>(table => table
            .Column<string>("RoleName")
            .Column<int>("Count")).ConfigureAwait(false);

        return 3;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom3Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<OpenIdAuthorizationIndex>(table => table
            .AddColumn<DateTime>("CreationDate")).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .AddColumn<DateTime>("CreationDate")).ConfigureAwait(false);

        return 4;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom4Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<OpenIdApplicationIndex>(table => table
            .CreateIndex("IDX_OpenIdApplicationIndex_DocumentId",
                "DocumentId",
                "ApplicationId",
                "ClientId")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAuthorizationIndex>(table => table
            .CreateIndex("IDX_OpenIdAuthorizationIndex_DocumentId_ApplicationId",
                "DocumentId",
                "ApplicationId",
                "Status",
                "Subject")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAuthorizationIndex>(table => table
            .CreateIndex("IDX_OpenIdAuthorizationIndex_DocumentId_AuthorizationId",
                "DocumentId",
                "AuthorizationId",
                "Status",
                "Type",
                "CreationDate")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdScopeIndex>(table => table
            .CreateIndex("IDX_OpenIdScopeIndex_DocumentId",
                "DocumentId",
                "Name",
                "ScopeId")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_ApplicationId",
                "DocumentId",
                "ApplicationId",
                "Status",
                "Subject")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_AuthorizationId",
                "DocumentId",
                "AuthorizationId",
                "Status",
                "Type",
                "CreationDate",
                "ExpirationDate")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .CreateIndex("IDX_OpenIdTokenIndex_DocumentId_TokenId",
                "DocumentId",
                "TokenId",
                "ReferenceId")
        ).ConfigureAwait(false);

        return 5;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom5Async()
    {
        await SchemaBuilder.AlterIndexTableAsync<OpenIdAppByLogoutUriIndex>(table => table
            .CreateIndex("IDX_OpenIdAppByLogoutUriIndex_LogoutRedirectUri", "LogoutRedirectUri")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAppByRedirectUriIndex>(table => table
            .CreateIndex("IDX_OpenIdAppByRedirectUriIndex_RedirectUri", "RedirectUri")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAppByRoleNameIndex>(table => table
            .CreateIndex("IDX_OpenIdAppByRoleNameIndex_RoleName", "RoleName")
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdScopeByResourceIndex>(table => table
            .CreateIndex("IDX_OpenIdScopeByResourceIndex_Resource", "Resource")
        ).ConfigureAwait(false);

        return 6;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom6Async()
    {
        // Create all index tables with the new collection value.
        await SchemaBuilder.CreateMapIndexTableAsync<OpenIdTokenIndex>(table => table
            .Column<string>("TokenId", column => column.WithLength(48))
            .Column<string>("ApplicationId", column => column.WithLength(48))
            .Column<string>("AuthorizationId", column => column.WithLength(48))
            .Column<DateTime>("ExpirationDate")
            .Column<string>("ReferenceId")
            .Column<string>("Status")
            .Column<string>("Subject")
            .Column<string>("Type")
            .Column<DateTime>("CreationDate"),
            collection: OpenIdTokenCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdToken_ApplicationId",
                "DocumentId",
                "ApplicationId",
                "Status",
                "Subject"),
            collection: OpenIdTokenCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdToken_AuthorizationId",
                "DocumentId",
                "AuthorizationId",
                "Status",
                "Type",
                "CreationDate",
                "ExpirationDate"),
            collection: OpenIdTokenCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdTokenIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdToken_TokenId",
                "DocumentId",
                "TokenId",
                "ReferenceId"),
            collection: OpenIdTokenCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateMapIndexTableAsync<OpenIdAuthorizationIndex>(table => table
            .Column<string>("AuthorizationId", column => column.WithLength(48))
            .Column<string>("ApplicationId", column => column.WithLength(48))
            .Column<string>("Status")
            .Column<string>("Subject")
            .Column<string>("Type")
            .Column<DateTime>("CreationDate"),
            collection: OpenIdAuthorizationCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAuthorizationIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdAuthorization_ApplicationId",
                "DocumentId",
                "ApplicationId",
                "Status",
                "Subject"),
            collection: OpenIdAuthorizationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAuthorizationIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdAuthorization_AuthorizationId",
                "DocumentId",
                "AuthorizationId",
                "Status",
                "Type",
                "CreationDate"),
            collection: OpenIdAuthorizationCollection
        ).ConfigureAwait(false);

        // Retrieve all existing tokens and authorizations from original Document table.
        var tokens = await _session.Query<OpenIdToken>().ListAsync().ConfigureAwait(false);
        var authorizations = await _session.Query<OpenIdAuthorization>().ListAsync().ConfigureAwait(false);

        // Enlist the old documents in the new collection and remove from the old collections.
        foreach (var token in tokens)
        {
            // Set the id to 0 or it will be considered an updated entity.
            token.Id = 0;
            await _session.SaveAsync(token, collection: OpenIdTokenCollection).ConfigureAwait(false);

            // Delete from the original collection.
            _session.Delete(token);
        }

        // Enlist the old documents in the new collection and remove from the old collections.
        foreach (var authorization in authorizations)
        {
            // Set the id to 0 or it will be considered an updated entity.
            authorization.Id = 0;
            await _session.SaveAsync(authorization, collection: OpenIdTokenCollection).ConfigureAwait(false);

            // Delete from the original collection.
            _session.Delete(authorization);
        }

        // This can be safely dropped here as the index provider now only writes to the new collection table.
        await SchemaBuilder.DropMapIndexTableAsync<OpenIdTokenIndex>().ConfigureAwait(false);
        await SchemaBuilder.DropMapIndexTableAsync<OpenIdAuthorizationIndex>().ConfigureAwait(false);

        return 7;
    }

    // This code can be removed in a later version.
    public async Task<int> UpdateFrom7Async()
    {
        // Create all index tables with the new collection value.
        await SchemaBuilder.CreateMapIndexTableAsync<OpenIdApplicationIndex>(table => table
            .Column<string>("ApplicationId", column => column.WithLength(48))
            .Column<string>("ClientId", column => column.Unique()),
            collection: OpenIdApplicationCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdApplicationIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdApplication",
                "DocumentId",
                "ApplicationId",
                "ClientId"),
            collection: OpenIdApplicationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdAppByLogoutUriIndex>(table => table
            .Column<string>("LogoutRedirectUri")
            .Column<int>("Count"),
            collection: OpenIdApplicationCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAppByLogoutUriIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdAppByLogoutUri_LogoutRedirectUri", "LogoutRedirectUri"),
            collection: OpenIdApplicationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdAppByRedirectUriIndex>(table => table
            .Column<string>("RedirectUri")
            .Column<int>("Count"),
            collection: OpenIdApplicationCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAppByRedirectUriIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdAppByRedirectUri_RedirectUri", "RedirectUri"),
            collection: OpenIdApplicationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdAppByRoleNameIndex>(table => table
            .Column<string>("RoleName")
            .Column<int>("Count"),
            collection: OpenIdApplicationCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdAppByRoleNameIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdAppByRoleName_RoleName", "RoleName"),
            collection: OpenIdApplicationCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateMapIndexTableAsync<OpenIdScopeIndex>(table => table
            .Column<string>("Name", column => column.Unique())
            .Column<string>("ScopeId", column => column.WithLength(48)),
            collection: OpenIdScopeCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdScopeIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdScope",
                "DocumentId",
                "Name",
                "ScopeId"),
            collection: OpenIdScopeCollection
        ).ConfigureAwait(false);

        await SchemaBuilder.CreateReduceIndexTableAsync<OpenIdScopeByResourceIndex>(table => table
            .Column<string>("Resource")
            .Column<int>("Count"),
            collection: OpenIdScopeCollection).ConfigureAwait(false);

        await SchemaBuilder.AlterIndexTableAsync<OpenIdScopeByResourceIndex>(table => table
            .CreateIndex("IDX_COL_OpenIdScopeByResource_Resource", "Resource"),
            collection: OpenIdScopeCollection
        ).ConfigureAwait(false);

        // Retrieve all existing applications and scopes from original Document table.
        var applications = await _session.Query<OpenIdApplication>().ListAsync().ConfigureAwait(false);
        var scopes = await _session.Query<OpenIdScope>().ListAsync().ConfigureAwait(false);

        // Enlist the old documents in the new collection and remove from the old collections.
        foreach (var application in applications)
        {
            // Set the id to 0 or it will be considered an updated entity.
            application.Id = 0;
            await _session.SaveAsync(application, collection: OpenIdApplicationCollection).ConfigureAwait(false);

            // Delete from the original collection.
            _session.Delete(application);
        }

        // Enlist the old documents in the new collection and remove from the old collections.
        foreach (var scope in scopes)
        {
            // Set the id to 0 or it will be considered an updated entity.
            scope.Id = 0;
            await _session.SaveAsync(scope, collection: OpenIdScopeCollection).ConfigureAwait(false);

            // Delete from the original collection.
            _session.Delete(scope);
        }

        // Flush the saved documents so that the old reduced indexes will be calculated
        // and committed to the transaction before they are then dropped.
        await _session.FlushAsync().ConfigureAwait(false);

        // These can be safely dropped after flushing.
        await SchemaBuilder.DropMapIndexTableAsync<OpenIdApplicationIndex>().ConfigureAwait(false);
        await SchemaBuilder.DropReduceIndexTableAsync<OpenIdAppByLogoutUriIndex>().ConfigureAwait(false);
        await SchemaBuilder.DropReduceIndexTableAsync<OpenIdAppByRedirectUriIndex>().ConfigureAwait(false);
        await SchemaBuilder.DropReduceIndexTableAsync<OpenIdAppByRoleNameIndex>().ConfigureAwait(false);

        await SchemaBuilder.DropMapIndexTableAsync<OpenIdScopeIndex>().ConfigureAwait(false);
        await SchemaBuilder.DropReduceIndexTableAsync<OpenIdScopeByResourceIndex>().ConfigureAwait(false);

        return 8;
    }
}
