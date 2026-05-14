using System.Security.Claims;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

/// <summary>
/// Default implementation if <see cref="IPermissionGrantingService"/>.
/// It is responsible for checking implied permissions.
/// </summary>
public class DefaultPermissionGrantingService : IPermissionGrantingService
{
    public bool IsGranted(PermissionRequirement requirement, IEnumerable<Claim> claims)
    {
        if (claims == null || !claims.Any())
        {
            return false;
        }

        var grantingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        GetGrantingNamesInternal(requirement.Permission, grantingNames);

        return claims.Any(claim => string.Equals(claim.Type, Permission.ClaimType, StringComparison.OrdinalIgnoreCase)
            && grantingNames.Contains(claim.Value));
    }

    private static void GetGrantingNamesInternal(Permission permission, HashSet<string> stack)
    {
        // The given name is tested
        if (!stack.Add(permission.Name))
        {
            // Avoid potential recursion
            return;
        }

        // Iterate implied permissions to grant, it present
        if (permission.ImpliedBy != null)
        {
            foreach (var impliedBy in permission.ImpliedBy)
            {
                if (impliedBy == null)
                {
                    continue;
                }

                // Otherwise accumulate the implied permission names recursively
                GetGrantingNamesInternal(impliedBy, stack);
            }
        }
    }
}
