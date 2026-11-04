using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.RemoteManagement;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.OpenId;

internal sealed class TenantRemoteManagementConfigurationService : IRemoteManagementTenantConfigurationService
{
    private readonly RemoteManagementConfigurationService _configurationService;
    private readonly IServiceProvider _serviceProvider;

    public TenantRemoteManagementConfigurationService(
        RemoteManagementConfigurationService configurationService,
        IServiceProvider serviceProvider)
    {
        _configurationService = configurationService;
        _serviceProvider = serviceProvider;
    }

    public async Task ConfigureAsync()
    {
        await _configurationService.ConfigureAsync();

        var roleService = _serviceProvider.GetService<IRoleService>();
        var roleStore = _serviceProvider.GetService<IRoleStore<IRole>>();
        if (roleService is null || roleStore is null)
        {
            return;
        }

        foreach (var role in (await roleService.GetRolesAsync()).OfType<Role>())
        {
            if (!await roleService.IsAdminRoleAsync(role.RoleName) ||
                role.RoleClaims.Any(claim =>
                    claim.ClaimType == Permission.ClaimType &&
                    claim.ClaimValue == RemoteManagementPermissions.AccessRemoteManagement.Name))
            {
                continue;
            }

            role.RoleClaims.Add(RoleClaim.Create(RemoteManagementPermissions.AccessRemoteManagement.Name));
            await roleStore.UpdateAsync(role, CancellationToken.None);
        }
    }
}
