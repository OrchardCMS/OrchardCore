using Microsoft.AspNetCore.Identity;
using OrchardCore.Data.Migration;
using OrchardCore.Indexing.Core;
using OrchardCore.Security;
using OrchardCore.Security.Services;

namespace OrchardCore.Search.Elasticsearch.Migrations;

public class PermissionMigrations : DataMigration
{
    private readonly IRoleService _roleService;
    private readonly IRoleStore<IRole> _roleStore;

    public PermissionMigrations(IRoleService roleService, IRoleStore<IRole> roleStore)
    {
        _roleService = roleService;
        _roleStore = roleStore;
    }

    public async Task<int> CreateAsync()
    {
        foreach (var roleToUpdate in ReplaceObsoletePermissions(await _roleService.GetRolesAsync()))
        {
            await _roleStore.UpdateAsync(roleToUpdate, CancellationToken.None);
        }

        return 1;
    }

    /// <summary>
    /// Selects the roles that need to be updated, and replaces their <c>QueryElasticsearch{0}Index</c> permissions with
    /// the equivalent <c>QueryIndex_{0}</c> permissions. 
    /// </summary>
    private static List<Role> ReplaceObsoletePermissions(IEnumerable<IRole> allRoles)
    {
        var rolesToUpdate = allRoles
            .Where(role => role is Role)
            .Cast<Role>()
            .Where(role => role.RoleClaims.Any(ElasticsearchIndexPermissionHelper.IsElasticsearchIndexPermissionClaim))
            .ToList();

        foreach (var role in rolesToUpdate)
        {
            foreach (var claim in role.RoleClaims.Where(ElasticsearchIndexPermissionHelper.IsElasticsearchIndexPermissionClaim))
            {
                var name = ElasticsearchIndexPermissionHelper.GetIndexNameFromPermissionName(claim.ClaimValue);
                claim.ClaimValue = IndexingPermissions.PermissionNamePrefix + name;
            }
        }

        return rolesToUpdate;
    }
}
