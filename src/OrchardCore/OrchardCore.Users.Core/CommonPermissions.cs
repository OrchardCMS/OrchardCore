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

        public static readonly Permission EditUsers = new(nameof(EditUsers), "Edit any user", new[] { ManageUsers }, true);
        public static readonly Permission DeleteUsers = new(nameof(DeleteUsers), "Delete any user", new[] { ManageUsers }, true);
        public static readonly Permission ListUsers = new(nameof(ListUsers), "List all Users", new[] { EditUsers, DeleteUsers });
        public static readonly Permission AssignRole = new(nameof(AssignRole), "Assign any role", new[] { EditUsers }, true);
        public static readonly Permission ManageUserProfileSettings = new(nameof(ManageUserProfileSettings), "Manage user profile settings", new[] { ManageUsers }, true);

        public static Permission CreateListUsersInRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("ListUsersInRole_{0}", "List users in role - {0}", new[] { ListUsers }));

        public static Permission CreateEditUsersInRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("EditUsersInRole_{0}", "Edit users in role - {0}", new[] { EditUsers }, true));

        public static Permission CreateDeleteUsersInRolePermission(string name) =>
            CreateDynamicPermission(name, new Permission("DeleteUsersInRole_{0}", "Delete users in role - {0}", new[] { DeleteUsers }, true));

        public static Permission CreateAssignUserToRolePermission(string name) =>
            CreatePermissionForManageUsersInRole(name);

        // Dynamic permission template.
        private static readonly Permission AssignUserToRole = new("ManageUsersInRole_{0}", "Assign users to {0} role", new[] { AssignRole }, true);

        private static Permission CreatePermissionForManageUsersInRole(string name)
            => new(
                    String.Format(AssignUserToRole.Name, name),
                    String.Format(AssignUserToRole.Description, name),
                    AssignUserToRole.ImpliedBy
                );

        private static Permission CreateDynamicPermission(string name, Permission permission)
            => new(
                String.Format(permission.Name, name),
                String.Format(permission.Description, name),
                permission.ImpliedBy,
                permission.IsSecurityCritical
            );
    }
}
