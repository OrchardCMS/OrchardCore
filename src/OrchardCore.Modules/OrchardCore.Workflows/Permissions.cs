using System.Collections.Generic;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Workflows
{
    public class Permissions : IPermissionProvider
    {
        public static readonly Permission ManageWorkflows = new Permission("ManageWorkflows", "Manage workflows");
        public static readonly Permission ExecuteWorkflows = new Permission("ExecuteWorkflows", "Execute workflows");

        public IEnumerable<Permission> GetPermissions()
        {
            return new Permission[] { ManageWorkflows, ExecuteWorkflows };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes()
        {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] { ManageWorkflows, ExecuteWorkflows }
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] { ManageWorkflows, ExecuteWorkflows }
                }
            };
        }
    }
}