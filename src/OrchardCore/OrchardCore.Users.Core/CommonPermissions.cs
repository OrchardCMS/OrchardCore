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
        public static readonly Permission ViewUsers = new("View Users", "View Users", new[] { ManageUsers });


        public static readonly Permission EditUsers = new(nameof(EditUsers), "Edit Any Users", new[] { ManageUsers }, true);
        public static readonly Permission DeleteUsers = new(nameof(DeleteUsers), "Delete Any Users", new[] { ManageUsers }, true);
        public static readonly Permission ListUsers = new(nameof(ListUsers), "List All Users", new[] { EditUsers, DeleteUsers });
        public static readonly Permission AssignRole = new(nameof(AssignRole), "Assign any role", new[] { EditUsers }, true);
        public static readonly Permission ManageUserProfileSettings = new(nameof(ManageUserProfileSettings), "Manage user profile settings", new[] { ManageUsers }, true);

        private static readonly Permission ListUsersInRole = new("listUsersInRole_{0}", "List Users in Role - {0}", new[] { ListUsers });
        private static readonly Permission EditUsersInRole = new("EditUsersInRole_{0}", "Edit Users in Role - {0}", new[] { EditUsers }, true);
        private static readonly Permission DeleteUsersInRole = new("DeleteUsersInRole_{0}", "Delete Users in Role - {0}", new[] { DeleteUsers }, true);
        private static readonly Permission AssignUserToRole = new("AssignUserToRole_{0}", "Assign users to {0} role", new[] { AssignRole }, true);


        public static Permission CreateListUsersInRolePermission(string name) => CreateDynamicPermission(name, ListUsersInRole);
        public static Permission CreateEditUsersInRolePermission(string name) => CreateDynamicPermission(name, EditUsersInRole);
        public static Permission CreateDeleteUsersInRolePermission(string name) => CreateDynamicPermission(name, DeleteUsersInRole);
        public static Permission CreateAssignUserToRolePermission(string name) => CreatePermissionForManageUsersInRole(name);


        // Dynamic permission template.
        private static readonly Permission ManageUsersInRole = new("ManageUsersInRole_{0}", "Manage Users in Role - {0}");

        private static Permission CreatePermissionForManageUsersInRole(string name)
            => new(
                    String.Format(AssignUserToRole.Name, name),
                    String.Format(AssignUserToRole.Description, name),
                    new[] {
                        AssignRole,
                        // this permission provides backward compatibility
                        new Permission(String.Format(ManageUsersInRole.Name, name))
                    }
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
