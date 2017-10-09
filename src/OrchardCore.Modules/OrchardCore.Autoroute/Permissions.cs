using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Autoroute
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission SetHomepage = new Permission("SetHomepage", "Set homepage.");
        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                SetHomepage
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = GetPermissions()
                }
            };
        }
    }
}
