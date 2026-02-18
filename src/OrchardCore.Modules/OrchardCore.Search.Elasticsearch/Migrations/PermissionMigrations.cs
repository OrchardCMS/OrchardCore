using Microsoft.AspNetCore.Identity;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using static OrchardCore.Search.Elasticsearch.ElasticsearchIndexPermissionHelper;

namespace OrchardCore.Search.Elasticsearch.Migrations;

public class PermissionMigrations : DataMigration
{
    private readonly IIndexProfileManager _indexProfileManager;
    private readonly IRoleService _roleService;
    private readonly IRoleStore<IRole> _roleStore;

    public override bool SkipIfInitializing => true;

    public PermissionMigrations(
        IIndexProfileManager indexProfileManager,
        IRoleService roleService,
        IRoleStore<IRole> roleStore)
    {
        _indexProfileManager = indexProfileManager;
        _roleService = roleService;
        _roleStore = roleStore;
    }

    public async Task<int> CreateAsync()
    {
        await ReplaceObsoletePermissionsAsync();

        return 1;
    }

    /// <summary>
    /// Selects the roles that need to be updated, and replaces their <c>QueryElasticsearch{0}Index</c> permissions with
    /// the equivalent <c>QueryIndex_{0}</c> permissions. 
    /// </summary>
    private async Task ReplaceObsoletePermissionsAsync()
    {
        var allRoles = await _roleService.GetRolesAsync();
        var rolesToUpdate = allRoles
            .Where(role => role is Role)
            .Cast<Role>()
            .Where(role => role.RoleClaims.Any(IsElasticsearchIndexPermissionClaim))
            .ToList();

        foreach (var role in rolesToUpdate)
        {
            foreach (var claim in role.RoleClaims.Where(IsElasticsearchIndexPermissionClaim))
            {
                var name = GetIndexNameFromPermissionName(claim.ClaimValue);
                var indexProfile = await _indexProfileManager.FindByNameAndProviderAsync(
                    name,
                    ElasticsearchConstants.ProviderName);

                if (indexProfile != null)
                {
                    claim.ClaimValue = IndexingPermissions.CreateDynamicPermission(indexProfile).Name;
                }
            }
            
            await _roleStore.UpdateAsync(role, CancellationToken.None);
        }
    }
}
