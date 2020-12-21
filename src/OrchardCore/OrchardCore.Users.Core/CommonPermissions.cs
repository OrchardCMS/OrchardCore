using System;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users
{
    public class CommonPermissions
    {
        public static readonly Permission ManageUsers = new Permission("ManageUsers", "Manage Users of all Roles");
        public static readonly Permission ManageUsersOfEmptyRoles = new Permission("ManageUsersOfEmptyRoles", "Manage Users with no Roles");

        private static readonly Permission ManageUsersOfRole = new Permission("ManageUsersOfRole_{0}", "Manage Users Of Role - {0}", new[] { ManageUsers });

        public static Permission CreatePermissionForManageUsersOfRole(string name)
            => new Permission(
                    String.Format(ManageUsersOfRole.Name, name),
                    String.Format(ManageUsersOfRole.Description, name),
                    ManageUsersOfRole.ImpliedBy
                );

    }
}
