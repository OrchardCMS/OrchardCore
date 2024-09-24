using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Infrastructure.Security;
using OrchardCore.Security;
using OrchardCore.Users;
using OrchardCore.Users.Services;

namespace OrchardCore.Roles;

public class RoleClaimsProvider : IUserClaimsProvider
{
    private readonly UserManager<IUser> _userManager;
    private readonly RoleManager<IRole> _roleManager;
    private readonly IdentityOptions _identityOptions;

    public RoleClaimsProvider(
        UserManager<IUser> userManager,
        RoleManager<IRole> roleManager,
        IOptions<IdentityOptions> identityOptions)
    {
        _userManager = userManager;
        _roleManager = roleManager;
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

        if (roles.Count == 0 || roles.Any(role => role.Type == RoleType.Owner))
        {
            return;
        }

        foreach (var role in roles)
        {
            claims.AddClaims(await _roleManager.GetClaimsAsync(role));
        }
    }
}
