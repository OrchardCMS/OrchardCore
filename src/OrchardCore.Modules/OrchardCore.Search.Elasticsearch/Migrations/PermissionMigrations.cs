using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using static OrchardCore.Search.Elasticsearch.ElasticsearchIndexPermissionHelper;

namespace OrchardCore.Search.Elasticsearch.Migrations;

public class PermissionMigrations : DataMigration, IDataMigrationWithCreate
{
    public int Create()
    {
        ShellScope.AddDeferredTask(ReplaceObsoletePermissionsAsync);

        return 1;
    }

    /// <summary>
    /// Selects the roles that need to be updated, and replaces their <c>QueryElasticsearch{0}Index</c> permissions with
    /// the equivalent <c>QueryIndex_{0}</c> permissions. 
    /// </summary>
    private static async Task ReplaceObsoletePermissionsAsync(ShellScope shellScope)
    {
        var indexProfileManager = shellScope.ServiceProvider.GetRequiredService<IIndexProfileManager>();
        var roleService = shellScope.ServiceProvider.GetRequiredService<IRoleService>();
        var roleStore = shellScope.ServiceProvider.GetRequiredService<IRoleStore<IRole>>();
        
        var allRoles = await roleService.GetRolesAsync();
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
                var indexProfile = await indexProfileManager.FindByNameAndProviderAsync(
                    name,
                    ElasticsearchConstants.ProviderName);

                if (indexProfile != null)
                {
                    claim.ClaimValue = IndexingPermissions.CreateDynamicPermission(indexProfile).Name;
                }
            }
            
            await roleStore.UpdateAsync(role, CancellationToken.None);
        }
    }
}
