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
        public static readonly Permission ManageUsers = new("ManageUsers", "Manage security settings and all users", true);

        /// <summary>
        /// View users only allows listing or viewing a users profile.
        /// </summary>
        public static readonly Permission ViewUsers = new("View Users", "View user's profile", new[] { ManageUsers });

        public static readonly Permission EditUsers = new("EditUsers", "Edit any user", new[] { ManageUsers }, true);

        public static readonly Permission DeleteUsers = new("DeleteUsers", "Delete any user", new[] { ManageUsers }, true);

        public static readonly Permission ListUsers = new("ListUsers", "List all Users", new[] { EditUsers, DeleteUsers });

        public static readonly Permission AssignRole = new("AssignRole", "Assign any role", new[] { EditUsers }, true);

        public static Permission CreateListUsersInRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("ListUsersInRole_{0}", "List users in role - {0}", new[] { ListUsers }));

        public static Permission CreateEditUsersInRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("EditUsersInRole_{0}", "Edit users in role - {0}", new[] { EditUsers }, true));

        public static Permission CreateDeleteUsersInRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("DeleteUsersInRole_{0}", "Delete users in role - {0}", new[] { DeleteUsers }, true));

        public static Permission CreateAssignUsersToRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("AssignUsersInRole_{0}", "Assign users in role - {0}", new[] { AssignRole }, true));

        public static Permission CreatePermissionForManageUsersInRole(string name) =>
            CreateDynamicPermission(name, new Permission("ManageUsersInRole_{0}", "Manage users in {0} role", new[] { ManageUsers }, true));

        // Dynamic permission template.

        private static Permission CreateDynamicPermission(string name, Permission permission)
            => new(
                String.Format(permission.Name, name),
                String.Format(permission.Description, name),
                permission.ImpliedBy,
                permission.IsSecurityCritical
            );
    }
}
