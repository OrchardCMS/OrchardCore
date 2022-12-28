using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public class SuperAdminClaimsProviders : IUserClaimsProvider
{
    private readonly IServiceProvider _serviceProvider;
    private ISiteService _siteService;
    private IEnumerable<IPermissionProvider> _permissionProviders;
    private IRoleStore<IRole> _roleStore;

    public SuperAdminClaimsProviders(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task GenerateAsync(IUser user, ClaimsIdentity claims)
    {
        _roleStore = _serviceProvider.GetService<IRoleStore<IRole>>();
        _siteService ??= _serviceProvider.GetService<ISiteService>();
        _permissionProviders ??= _serviceProvider.GetServices<IPermissionProvider>();

        // NullRoleStore is only registered when Roles features is disabled.
        // So we load all available claims for the super user only to grant them to all available permissions.
        // These provider should only apply when NullRoleStore is registered.
        if (
            _roleStore is not NullRoleStore
            || user is not User su
            || _siteService == null
            || _permissionProviders == null
            || !_permissionProviders.Any())
        {
            return;
        }

        var site = await _siteService.GetSiteSettingsAsync();

        if (site.SuperUser != null && String.Equals(site.SuperUser, su.UserId))
        {
            var permissionCollections = await Task.WhenAll(_permissionProviders.Select(x => x.GetPermissionsAsync()));

            var permissionClaims = permissionCollections.SelectMany(permissionCollection => permissionCollection)
                .Select(permission => new Claim(Permission.ClaimType, permission.Name));

            claims.AddClaims(permissionClaims);
        }
    }
}
