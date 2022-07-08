using System;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles
{
    public class CommonPermissions
    {
        public static readonly Permission ManageRoles = new Permission("ManageRoles", "Managing Roles", isSecurityCritical: true);
        public static readonly Permission AssignRoles = new Permission("AssignRoles", "Assign Roles", new[] { ManageRoles }, isSecurityCritical: true);

        /// <summary>
        /// Dynamic permission template for assign role
        /// </summary>
        private static readonly Permission AssignRole = new Permission("AssignRole_{0}", "Assign Role - {0}", new[] { AssignRoles, ManageRoles });

        public static Permission CreatePermissionForAssignRole(string name)
            => new Permission(
                    String.Format(AssignRole.Name, name),
                    String.Format(AssignRole.Description, name),
                    AssignRole.ImpliedBy
                );
    }
}
