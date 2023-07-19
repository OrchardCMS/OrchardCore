using System;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles
{
    public class CommonPermissions
    {
        public static readonly Permission ManageRoles = new("ManageRoles", "Managing Roles", isSecurityCritical: true);
        public static readonly Permission AssignRoles = new("AssignRoles", "Assign Roles", new[] { ManageRoles }, isSecurityCritical: true);

        /// <summary>
        /// Dynamic permission template for assign role.
        /// </summary>
        private static readonly Permission _assignRole = new("AssignRole_{0}", "Assign Role - {0}", new[] { AssignRoles, ManageRoles });

        public static Permission CreatePermissionForAssignRole(string name) =>
            new(
                String.Format(_assignRole.Name, name),
                String.Format(_assignRole.Description, name),
                _assignRole.ImpliedBy
            );
    }
}
