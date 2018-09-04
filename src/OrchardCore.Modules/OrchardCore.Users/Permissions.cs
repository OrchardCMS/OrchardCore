using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageUsers = new Permission("ManageUsers", "Managing Users");
        public static readonly Permission Login = new Permission("Login", "Ability to log in");
        public static readonly Permission ResetPassword = new Permission("Login", "Ability to log in");

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
