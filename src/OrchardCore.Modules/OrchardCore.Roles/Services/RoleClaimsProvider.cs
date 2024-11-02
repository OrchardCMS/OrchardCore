using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Security;
using OrchardCore.Users;
using OrchardCore.Users.Services;

namespace OrchardCore.Roles;

public class RoleClaimsProvider : IUserClaimsProvider
{
    private readonly UserManager<IUser> _userManager;
    private readonly RoleManager<IRole> _roleManager;
    private readonly ISystemRoleNameProvider _systemRoleNameProvider;
    private readonly IdentityOptions _identityOptions;

    public RoleClaimsProvider(
        UserManager<IUser> userManager,
        RoleManager<IRole> roleManager,
        ISystemRoleNameProvider systemRoleNameProvider,
        IOptions<IdentityOptions> identityOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _systemRoleNameProvider = systemRoleNameProvider;
        _identityOptions = identityOptions.Value;
    }

    public async Task GenerateAsync(IUser user, ClaimsIdentity claims)
    {
        if (!_userManager.SupportsUserRole)
        {
            return;
        }

        var isAdministrator = false;

        if (await _userManager.IsInRoleAsync(user, await _systemRoleNameProvider.GetAdminRoleAsync()))
        {
            claims.AddClaim(StandardClaims.SiteOwner);

            isAdministrator = true;
        }

        var roleNames = await _userManager.GetRolesAsync(user);

        foreach (var roleName in roleNames)
        {
            claims.AddClaim(new Claim(_identityOptions.ClaimsIdentity.RoleClaimType, roleName));

            if (isAdministrator || !_roleManager.SupportsRoleClaims)
            {
                continue;
            }

            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                continue;
            }

            claims.AddClaims(await _roleManager.GetClaimsAsync(role));
        }
    }
}
