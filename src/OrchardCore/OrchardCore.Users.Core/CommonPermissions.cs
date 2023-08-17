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
        /// Allows viewing user profiles.
        /// </summary>
        public static readonly Permission ViewUsers = new("View Users", "View user profiles", new[] { ManageUsers });

        public static readonly Permission EditUsers = new("EditUsers", "Edit any user", new[] { ManageUsers }, true);

        public static readonly Permission DeleteUsers = new("DeleteUsers", "Delete any user", new[] { ManageUsers }, true);

        public static readonly Permission ListUsers = new("ListUsers", "List all users", new[] { EditUsers, DeleteUsers });

        public static readonly Permission AssignRoleToUsers = new("AssignRoleToUsers", "Assign any role to users", new[] { EditUsers }, true);

        public static readonly Permission EditOwnUser = new("ManageOwnUserInformation", "Edit own user information", new Permission[] { EditUsers });

        public static Permission CreateEditUsersInRolePermission(string roleName) =>
            CreateDynamicPermission(roleName, new Permission("EditUsersInRole_{0}", "Edit users in {0} role", new[] { EditUsers }, true));

        public static Permission CreateDeleteUsersInRolePermission(string roleName) =>
            CreateDynamicPermission(roleName, new Permission("DeleteUsersInRole_{0}", "Delete users in {0} role", new[] { DeleteUsers }, true));

        public static Permission CreateListUsersInRolePermission(string roleName) =>
            CreateDynamicPermission(roleName, new Permission("ListUsersInRole_{0}", "List users in {0} role", new[] { ListUsers }));

        public static Permission CreateAssignRoleToUsersPermission(string roleName) =>
            CreateDynamicPermission(roleName, new Permission("AssignRoleToUsers_{0}", "Assign {0} role to users", new[] { AssignRoleToUsers }, true));

        public static Permission CreatePermissionForManageUsersInRole(string name) =>
            CreateDynamicPermission(name, new Permission("ManageUsersInRole_{0}", "Manage users in {0} role", new[] { ManageUsers }, true));

        // Dynamic permission template.
        private static Permission CreateDynamicPermission(string roleName, Permission permission)
            => new(
                String.Format(permission.Name, roleName),
                String.Format(permission.Description, roleName),
                permission.ImpliedBy,
                permission.IsSecurityCritical
            );
    }
}
