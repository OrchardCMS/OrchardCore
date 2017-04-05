using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.UserCenter
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission AccessUserCenterPanel = new Permission("AccessUserCenterPanel", "Access user center panel");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[]
            {
                AccessUserCenterPanel
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[]
            {
                new PermissionStereotype
                {
                    Name = "Administrator",
                    Permissions = GetPermissions()
                }
            };
        }
    }
}