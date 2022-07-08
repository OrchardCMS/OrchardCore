using System;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Users
{
    public class CommonPermissions
    {
        /// <summary>
        /// When authorizing request ManageUsers and pass an <see cref="IUser"/>
        /// Do not request a dynamic permission unless you are checking if the user can manage a specific role.
        /// </summary>
        public static readonly Permission ManageUsers = new Permission("ManageUsers", "Manage Users", true);

        /// <summary>
        /// View users only allows listing or viewing a users profile.
        /// </summary>
        public static readonly Permission ViewUsers = new Permission("View Users", "View Users", new[] { ManageUsers });

        // Dynamic permission template.
        private static readonly Permission ManageUsersInRole = new Permission("ManageUsersInRole_{0}", "Manage Users in Role - {0}", new[] { ManageUsers });

        public static Permission CreatePermissionForManageUsersInRole(string name)
            => new Permission(
                    String.Format(ManageUsersInRole.Name, name),
                    String.Format(ManageUsersInRole.Description, name),
                    ManageUsersInRole.ImpliedBy
                );
    }
}
