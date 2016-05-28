using System.Collections.Generic;
using Orchard.Security.Permissions;

namespace Orchard.Users
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageUsers = new Permission("ManageUsers", "Managing Users");

        public IEnumerable<Permission> GetPermissions()
        {
            return new[] {
                ManageUsers,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageUsers}
                },
            };
        }

    }
}
