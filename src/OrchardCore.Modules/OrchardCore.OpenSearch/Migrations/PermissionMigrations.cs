using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Indexing;
using OrchardCore.Indexing.Core;
using OrchardCore.Security;
using OrchardCore.Security.Services;
using static OrchardCore.OpenSearch.OpenSearchIndexPermissionHelper;

namespace OrchardCore.OpenSearch.Migrations;

internal sealed class PermissionMigrations : DataMigration
{
    private readonly ShellSettings _shellSettings;

    public PermissionMigrations(ShellSettings shellSettings) =>
        _shellSettings = shellSettings;

    public int Create()
    {
        if (!_shellSettings.IsInitializing())
        {
            ShellScope.AddDeferredTask(ReplaceObsoletePermissionsAsync);
        }

        return 1;
    }

    /// <summary>
    /// Selects the roles that need to be updated, and replaces their legacy OpenSearch index permissions with
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
            .Where(role => role.RoleClaims.Any(IsOpenSearchIndexPermissionClaim))
            .ToList();

        foreach (var role in rolesToUpdate)
        {
            foreach (var claim in role.RoleClaims.Where(IsOpenSearchIndexPermissionClaim))
            {
                var name = GetIndexNameFromPermissionName(claim.ClaimValue);
                var indexProfile = await indexProfileManager.FindByNameAndProviderAsync(
                    name,
                    OpenSearchConstants.ProviderName);

                if (indexProfile != null)
                {
                    claim.ClaimValue = IndexingPermissions.CreateDynamicPermission(indexProfile).Name;
                }
            }

            await roleStore.UpdateAsync(role, CancellationToken.None);
        }
    }
}
