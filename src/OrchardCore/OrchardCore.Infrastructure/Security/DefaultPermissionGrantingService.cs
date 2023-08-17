using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security
{
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

            // SiteOwner permission grants them all
            grantingNames.Add(StandardPermissions.SiteOwner.Name);

            return claims.Any(claim => String.Equals(claim.Type, Permission.ClaimType, StringComparison.OrdinalIgnoreCase)
                && grantingNames.Contains(claim.Value));
        }

        private void GetGrantingNamesInternal(Permission permission, HashSet<string> stack)
        {
            // The given name is tested
            stack.Add(permission.Name);

            // Iterate implied permissions to grant, it present
            if (permission.ImpliedBy != null && permission.ImpliedBy.Any())
            {
                foreach (var impliedBy in permission.ImpliedBy)
                {
                    // Avoid potential recursion
                    if (impliedBy == null || stack.Contains(impliedBy.Name))
                    {
                        continue;
                    }

                    // Otherwise accumulate the implied permission names recursively
                    GetGrantingNamesInternal(impliedBy, stack);
                }
            }
        }
    }
}
