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
        /// Allow the user to Edit other users.
        /// </summary>
        public static readonly Permission EditUsers = new Permission("EditUsers", "Edit Other Users", new[] { ManageUsers }, true);

        /// <summary>
        /// Allow the user to Delete other users.
        /// </summary>
        public static readonly Permission DeleteUsers = new Permission("DeleteUsers", "Delete Other Users", new[] { ManageUsers }, true);

        /// <summary>
        /// View users only allows listing or viewing a users profile.
        /// </summary>
        public static readonly Permission ViewUsers = new Permission("View Users", "View All Users", new[] { ManageUsers, EditUsers, DeleteUsers });

        // Dynamic permission template.
        private static readonly Permission ManageUsersInRole = new Permission("ManageUsersInRole_{0}", "Manage Users in Role - {0}", true);

        public static Permission CreatePermissionForManageUsersInRole(string name)
            => new Permission(
                    String.Format(ManageUsersInRole.Name, name),
                    String.Format(ManageUsersInRole.Description, name),
                    ManageUsersInRole.ImpliedBy
                );
    }
}
