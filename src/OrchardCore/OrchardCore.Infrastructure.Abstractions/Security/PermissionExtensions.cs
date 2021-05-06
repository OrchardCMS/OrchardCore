using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security
{
    public static class PermissionExtensions
    {
        /// <summary>
        /// Evaluates if the specified <see cref="Permission"/> is granted by provided claims.
        /// </summary>
        /// <param name="permission">The <see cref="Permission"/> to test</param>
        /// <param name="claims">Provided claims.</param>
        /// <returns>True if the permission is granted, otherwise false.</returns>
        public static bool IsGranted(this Permission permission, IEnumerable<Claim> claims)
        {
            if (claims == null || !claims.Any())
            {
                return false;
            }

            var grantingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            GetGrantingNamesInternal(permission, grantingNames);

            // SiteOwner permission grants them all
            grantingNames.Add(StandardPermissions.SiteOwner.Name);

            return claims.Any(claim => String.Equals(claim.Type, Permission.ClaimType, StringComparison.OrdinalIgnoreCase)
                && grantingNames.Contains(claim.Value));
        }

        private static void GetGrantingNamesInternal(this Permission permission, HashSet<string> stack)
        {
            // The given name is tested
            stack.Add(permission.Name);

            // Iterate implied permissions to grant, it present
            if (permission.ImpliedBy != null && permission.ImpliedBy.Any())
            {
                foreach (var impliedBy in permission.ImpliedBy)
                {
                    // Avoid potential recursion
                    if (stack.Contains(impliedBy.Name))
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
