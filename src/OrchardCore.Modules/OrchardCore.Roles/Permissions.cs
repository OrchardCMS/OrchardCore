using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageRoles = new Permission("ManageRoles", "Managing Roles", isSecurityCritical: true);

        public Task<IEnumerable<Permission>> GetPermissionsAsync()
        {
            return Task.FromResult(new[]
            {
                ManageRoles, StandardPermissions.SiteOwner
            }
            .AsEnumerable());
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageRoles, StandardPermissions.SiteOwner }
                },
            };
        }
    }
}
