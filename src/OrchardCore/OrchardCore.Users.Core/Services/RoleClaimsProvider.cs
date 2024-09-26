using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;
using OrchardCore.Users;
using OrchardCore.Users.Services;

namespace OrchardCore.Roles;

public class RoleClaimsProvider : IUserClaimsProvider
{
    private readonly UserManager<IUser> _userManager;
    private readonly RoleManager<IRole> _roleManager;
    private readonly ShellSettings _shellSettings;
    private readonly IdentityOptions _identityOptions;

    public RoleClaimsProvider(
        UserManager<IUser> userManager,
        RoleManager<IRole> roleManager,
        ShellSettings shellSettings,
        IOptions<IdentityOptions> identityOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _shellSettings = shellSettings;
        _identityOptions = identityOptions.Value;
    }

    public async Task GenerateAsync(IUser user, ClaimsIdentity claims)
    {
        if (!_userManager.SupportsUserRole)
        {
            return;
        }

        var roleNames = await _userManager.GetRolesAsync(user);
        var roles = new List<IRole>();

        foreach (var roleName in roleNames)
        {
            claims.AddClaim(new Claim(_identityOptions.ClaimsIdentity.RoleClaimType, roleName));

            if (!_roleManager.SupportsRoleClaims)
            {
                continue;
            }

            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                continue;
            }

            roles.Add(role);
        }
        var adminRoleName = _shellSettings.GetSystemAdminRoleName();

        if (roles.Count == 0 ||
            roles.Any(role => role.RoleName.Equals(adminRoleName, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        foreach (var role in roles)
        {
            claims.AddClaims(await _roleManager.GetClaimsAsync(role));
        }
    }
}
